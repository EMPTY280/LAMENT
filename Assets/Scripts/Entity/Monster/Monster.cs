using UnityEngine;

namespace LAMENT
{
    public abstract class Monster : Entity
    {
        [Header("드롭 테이블")]
        [SerializeField] private GameObject[] dropTable;

        protected override void TakeDamage(DamageResponse rsp)
        {
            hpCurr -= rsp.amount;

            if (!isDead && hpCurr <= 0)
            {
                isDead = true;
                OnDied();
            }
        }

        protected override void OnDied()
        {
            if (dropTable.Length <= 0)
                return;

            // TODO: 아이템 풀링 고려
            Instantiate(dropTable[Random.Range(0, dropTable.Length - 1)], transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}