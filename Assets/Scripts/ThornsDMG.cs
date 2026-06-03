using UnityEngine;

namespace LAMENT
{

    public class ThornsHazard : MonoBehaviour
    {
        [SerializeField] private float damage = 1f;    // 틱당 데미지
        [SerializeField] private float invincibleTime = 1.0f;  // 무적 시간 (초)
        [SerializeField] private float tickInterval = 1.0f;  // 무적 후 틱 간격 (초)

        private Player m_player = null;
        private float m_tickTimer = 0f;
        private bool m_playerInside = false;
        private bool m_isInvincible = false;

        private void Update()
        {
            if (!m_playerInside || m_player == null)
                return;

            m_tickTimer += Time.deltaTime;

            if (m_isInvincible)
            {
                // 무적 시간 체크
                if (m_tickTimer >= invincibleTime)
                {
                    m_isInvincible = false;
                    m_tickTimer = 0f;
                }
            }
            else
            {
                // 무적 끝난 후 틱 데미지
                if (m_tickTimer >= tickInterval)
                {
                    m_tickTimer = 0f;
                    ApplyDamage();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Player player))
                return;

            m_player = player;
            m_playerInside = true;
            m_isInvincible = true;
            m_tickTimer = 0f;

            // 닿는 순간 즉시 첫 데미지
            ApplyDamage();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Player player))
                return;

            if (player != m_player)
                return;

            m_player = null;
            m_playerInside = false;
            m_isInvincible = false;
            m_tickTimer = 0f;
        }

        private void ApplyDamage()
        {
            if (m_player == null)
                return;

            m_player.OnHitTaken(new DamageResponse
            {
                src = null,
                amount = damage
            });
        }
    }
}