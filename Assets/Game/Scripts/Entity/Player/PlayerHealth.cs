using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class PlayerHealth : MonoBehaviour
    {
         [Header("체력설정")]
        [SerializeField] private int initialMaxHp = 5;
        [SerializeField] private int minMaxHp = 1;

        [Header("위 게이지 설정")]
        [SerializeField] private int stomachMax = 100;
        [SerializeField] private int gainPerAttack = 5;
        [SerializeField] private int gainPerLimb = 20;

        public int CurrentHp { get; private set; }

        private int baseMaxHp;
        private int gutMaxHpBonus;

        public int CurrentMaxHp => Mathf.Max(minMaxHp, baseMaxHp + gutMaxHpBonus);
        public int InitialMaxHp => initialMaxHp;

        public int StomachCurr { get; private set; }

        public bool IsDead { get; private set; }
        public bool IsGameOver { get; private set; }

        private float gutStomachGainMult = 1f;

        private void Awake()
        {
            baseMaxHp = initialMaxHp;
            gutMaxHpBonus = 0;
            gutStomachGainMult = 1f;

            CurrentHp = CurrentMaxHp;
            StomachCurr = 0;

            GameManager.Eventbus.Subscribe<GEOnGutsRuntimeChanged>(OnGutsRuntimeChanged);

            PublishHealthChanged();
            PublishStomachChanged();
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnGutsRuntimeChanged>(OnGutsRuntimeChanged);
        }

        private void OnGutsRuntimeChanged(GEOnGutsRuntimeChanged e)
        {
            int prevMax = CurrentMaxHp;

            gutMaxHpBonus = e.MaxHpBonus;
            gutStomachGainMult = Mathf.Max(0f, e.StomachGainMult);

            if (CurrentHp > CurrentMaxHp)
                CurrentHp = CurrentMaxHp;

            if (prevMax != CurrentMaxHp)
                PublishHealthChanged();
        }

        public void TakeHit(int amount)
        {
            if (IsGameOver)
                return;

            CurrentHp -= amount;
            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                HandleDeath();
            }

            PublishHealthChanged();
        }

        private void HandleDeath()
        {
            IsDead = true;

            int remainingMaxHpAfter = CurrentMaxHp - 1;
            GameManager.Eventbus.Publish(new GEOnPlayerDied(remainingMaxHpAfter));

            if (CurrentMaxHp <= minMaxHp)
            {
                IsGameOver = true;
                GameManager.Eventbus.Publish(new GEOnPlayerGameOver());
                return;
            }

            baseMaxHp = Mathf.Max(minMaxHp - gutMaxHpBonus, baseMaxHp - 1);
            CurrentHp = CurrentMaxHp;
            IsDead = false;

            GameManager.Eventbus.Publish(new GEOnPlayerRevived(CurrentHp, CurrentMaxHp));
            PublishHealthChanged();
        }

        public void OnAttackLanded()
        {
            AddStomach(gainPerAttack);
        }

        public void OnLimbConsumed()
        {
            AddStomach(gainPerLimb);
        }

        private void AddStomach(int amount)
        {
            if (IsGameOver)
                return;

            int finalAmount = Mathf.RoundToInt(amount * gutStomachGainMult);
            if (finalAmount <= 0)
                return;

            StomachCurr += finalAmount;

            if (StomachCurr >= stomachMax)
            {
                StomachCurr -= stomachMax;

                if (baseMaxHp < initialMaxHp)
                {
                    baseMaxHp++;
                    CurrentHp = Mathf.Min(CurrentHp + 1, CurrentMaxHp);
                    PublishHealthChanged();
                }
            }

            PublishStomachChanged();
        }

        private void PublishHealthChanged()
        {
            GameManager.Eventbus.Publish(
                new GEOnPlayerHealthChanged(CurrentHp, CurrentMaxHp, InitialMaxHp));
        }

        private void PublishStomachChanged()
        {
            GameManager.Eventbus.Publish(
                new GEOnStomachGaugeChanged(StomachCurr, stomachMax));
        }
    }
}