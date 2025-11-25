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
        [SerializeField] private int gainPerAttack =5;
        [SerializeField] private int gainPerLimb = 20;

        public int CurrentHp{get; private set;}
         public int CurrentMaxHp { get; private set; }
        public int InitialMaxHp { get { return initialMaxHp; } }

        public int StomachCurr { get; private set; }

        public bool IsDead { get; private set; }
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            CurrentMaxHp = initialMaxHp;
            CurrentHp = CurrentMaxHp;
            StomachCurr = 0;

            PublishHealthChanged();
            PublishStomachChanged();
        }

        #region 데미지 & 사망 처리

        /// <summary>
        /// 한 번 맞았을 때 호출 (필요하면 amount 늘려도 됨)
        /// </summary>
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

            // 최대 체력이 1인 상태에서 죽으면 게임오버
            if (CurrentMaxHp <= minMaxHp)
            {
                IsGameOver = true;
                GameManager.Eventbus.Publish(new GEOnPlayerGameOver());
                return;
            }

            // 부활 : 최대 체력 -1, 그 값으로 풀피
            CurrentMaxHp = Mathf.Max(minMaxHp, CurrentMaxHp - 1);
            CurrentHp = CurrentMaxHp;
            IsDead = false;

            GameManager.Eventbus.Publish(new GEOnPlayerRevived(CurrentHp, CurrentMaxHp));
            PublishHealthChanged();
        }

        #endregion

        #region 위 게이지

        /// <summary> 적에게 공격이 적중했을 때 </summary>
        public void OnAttackLanded()
        {
            AddStomach(gainPerAttack);
        }

        /// <summary> 팔 / 다리 섭취했을 때 (아이템에서 호출) </summary>
        public void OnLimbConsumed()
        {
            AddStomach(gainPerLimb);
        }

        private void AddStomach(int amount)
        {
            if (IsGameOver)
                return;

            // 이미 최대 체력이 다 회복되었으면 더 이상 의미 없음
            if (CurrentMaxHp >= initialMaxHp)
                return;

            StomachCurr += amount;
            if (StomachCurr >= stomachMax)
            {
                StomachCurr -= stomachMax;

                if (CurrentMaxHp < initialMaxHp)
                {
                    CurrentMaxHp++;
                    // 위게이지로 최대 체력이 회복되면, 현재 체력도 1 회복
                    CurrentHp = Mathf.Min(CurrentHp + 1, CurrentMaxHp);
                    PublishHealthChanged();
                }
            }

            PublishStomachChanged();
        }

        #endregion

        #region 이벤트 발행

        private void PublishHealthChanged()
        {
            GameManager.Eventbus.Publish(
                new GEOnPlayerHealthChanged(CurrentHp, CurrentMaxHp, initialMaxHp));
        }

        private void PublishStomachChanged()
        {
            GameManager.Eventbus.Publish(
                new GEOnStomachGaugeChanged(StomachCurr, stomachMax));
        }

        #endregion
    }
}