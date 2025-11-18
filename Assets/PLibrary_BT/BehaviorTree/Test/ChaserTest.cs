using UnityEngine;

namespace PLibrary
{
    public class ChaserTest : MonoBehaviour
    {
        private BehaviorTree bt;

        public Transform target;
        public float detectionRadius;
        public float moveSpeed = 2;

        public float attackDelay = 1;
        public float attackRadius;
        private float lastAttackTime = 0;


        private void Awake()
        {
            lastAttackTime = Time.time;
            BuildBT();
        }

        /// <summary> 2D(즉 XY평면)에 원을 디버그로 그립니다. FROM AI </summary>
        private void DrawCircle(Vector2 center, float radius, Color color, float duration = 0, int segments = 36)
        {
            Vector3 startPoint = new Vector3(center.x + radius, center.y, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;

                Vector3 endPoint = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius,
                    0f
                );

                Debug.DrawLine(startPoint, endPoint, color, duration);
                startPoint = endPoint;
            }
        }

        private void Update()
        {
            DrawCircle(transform.position, detectionRadius, Color.green);
            DrawCircle(transform.position, attackRadius, Color.red);
            bt.Run();
        }

        /// <summary> BT 구성 </summary>
        private void BuildBT()
        {
            // 공격 <- 접근 <- 대기

            bt = new();

            BTSelectorNode root = new();
            bt.SetRootNode(root);

            // ===== 공격 시퀀스 =====

            BTSequenceNode attackSQ = new();
            root.AddChild(attackSQ);

            BTActionNode attackRangeCheck = new(IsTargetInAttackRadius);
            attackSQ.AddChild(attackRangeCheck);

            BTActionNode attackExe = new(Attack);
            attackSQ.AddChild(attackExe);

            // ===== 추적 시퀀스 =====

            BTSequenceNode chaseSQ = new();
            root.AddChild(chaseSQ);

            BTActionNode chaseRangeCheck = new(IsTargetInDetectionRadius);
            chaseSQ.AddChild(chaseRangeCheck);

            BTActionNode chaseExe = new(Chase);
            chaseSQ.AddChild(chaseExe);
        }

        private EBTState IsTargetInAttackRadius()
        {
            if (!target)
                return EBTState.FAILURE;

            if (Vector3.Distance(transform.position, target.position) <= attackRadius)
                return EBTState.SUCCESS;

            return EBTState.FAILURE;
        }

        private EBTState Attack()
        {
            if (attackDelay < Time.time - lastAttackTime)
            {
                lastAttackTime = Time.time;
                Debug.Log("ATTACK!");
            }
            return EBTState.RUN;
        }

        private EBTState IsTargetInDetectionRadius()
        {
            if (!target)
                return EBTState.FAILURE;

            if (Vector3.Distance(transform.position, target.position) <= detectionRadius)
                return EBTState.SUCCESS;

            return EBTState.FAILURE;
        }
    
        private EBTState Chase()
        {
            if (!target)
                return EBTState.FAILURE;

            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            return EBTState.RUN;
        }
    }
}