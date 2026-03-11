using UnityEngine;

namespace LAMENT
{
    public abstract class Monster : Entity
    {
        [SerializeField] protected Skill[] skills;

        [Header("드롭 테이블")]
        [SerializeField] private GameObject[] dropTable;

        [Header("AI")]
        [SerializeField] protected Transform target;


        protected override void Awake()
        {
            base.Awake();

            InitTarget();
            InitSkills();
        }

        private void InitTarget()
        {
            // 직접 배정한 타겟이 있다면 패스
            if (target)
                return;

            // TODO: 더 성능 좋은 코드로 교체
            GameObject[] rst = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject g in rst)
            {
                if (g.TryGetComponent(out Player player))
                {
                    target = player.transform;
                    break;
                }
            }
        }

        private void InitSkills()
        {
            for (int i = 0; i < skills.Length; i++)
                TryAddEffector(skills[i]);
        }

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
            // TODO: 아이템 풀링 고려
            if (0 < dropTable.Length)
                Instantiate(dropTable[Random.Range(0, dropTable.Length - 1)], transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}