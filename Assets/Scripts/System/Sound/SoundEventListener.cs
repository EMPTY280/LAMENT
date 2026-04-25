using UnityEngine;

namespace LAMENT
{
    public class SoundEventListener : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private string playerAttackSoundId = "SFX_PLAYER_ATTACK";
        [SerializeField] private string playerHitSoundId = "SFX_PLAYER_HIT";
        [SerializeField] private string playerDeadSoundId = "SFX_PLAYER_DIE";

        [Header("QTE")]
        [SerializeField] private string qteStartSoundId = "SFX_QTE_START";
        [SerializeField] private string qteSuccessSoundId = "SFX_QTE_SUCCESS";
        [SerializeField] private string qteFailSoundId = "SFX_QTE_FAIL";

        [Header("UI / Shop")]
        [SerializeField] private string purchaseSoundId = "UI_PURCHASE";
        [SerializeField] private string moneySoundId = "UI_MONEY";

        private int lastPlayerHp = -1;

        private void OnEnable()
        {
            GameManager.Eventbus.Subscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedEquipment);
            GameManager.Eventbus.Subscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Subscribe<GEOnPlayerGameOver>(OnPlayerGameOver);

            GameManager.Eventbus.Subscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Subscribe<GEOnQTESucceeded>(OnQTESucceeded);
            GameManager.Eventbus.Subscribe<GEOnQTEFailed>(OnQTEFailed);

            GameManager.Eventbus.Subscribe<GEOnGutPurchased>(OnGutPurchased);
            GameManager.Eventbus.Subscribe<GEOnMoneyChanged>(OnMoneyChanged);
        }

        private void OnDisable()
        {
            GameManager.Eventbus.Unsubscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedEquipment);
            GameManager.Eventbus.Unsubscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Unsubscribe<GEOnPlayerGameOver>(OnPlayerGameOver);

            GameManager.Eventbus.Unsubscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Unsubscribe<GEOnQTESucceeded>(OnQTESucceeded);
            GameManager.Eventbus.Unsubscribe<GEOnQTEFailed>(OnQTEFailed);

            GameManager.Eventbus.Unsubscribe<GEOnGutPurchased>(OnGutPurchased);
            GameManager.Eventbus.Unsubscribe<GEOnMoneyChanged>(OnMoneyChanged);
        }

        private void OnPlayerUsedEquipment(GEOnPlayerUsedEquiment e)
        {
            PlaySFX(playerAttackSoundId);
        }

        private void OnPlayerHealthChanged(GEOnPlayerHealthChanged e)
        {
            if (lastPlayerHp < 0)
            {
                lastPlayerHp = e.Curr;
                return;
            }

            if (e.Curr < lastPlayerHp)
                PlaySFX(playerHitSoundId);

            lastPlayerHp = e.Curr;
        }

        private void OnPlayerGameOver(GEOnPlayerGameOver e)
        {
            PlaySFX(playerDeadSoundId);
        }

        private void OnQTEStarted(GEOnQTEStarted e)
        {
            PlaySFX(qteStartSoundId);
        }

        private void OnQTESucceeded(GEOnQTESucceeded e)
        {
            PlaySFX(qteSuccessSoundId);
        }

        private void OnQTEFailed(GEOnQTEFailed e)
        {
            PlaySFX(qteFailSoundId);
        }

        private void OnGutPurchased(GEOnGutPurchased e)
        {
            PlayUI(purchaseSoundId);
        }

        private void OnMoneyChanged(GEOnMoneyChanged e)
        {
            if (e.Delta > 0)
                PlayUI(moneySoundId);
        }

        private void PlaySFX(string id)
        {
            if (!SoundManager.Instance)
                return;

            SoundManager.Instance.PlaySFX(id);
        }

        private void PlayUI(string id)
        {
            if (!SoundManager.Instance)
                return;

            SoundManager.Instance.PlayUI(id);
        }
    }
}