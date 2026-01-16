using System;
using PLibrary;
using UnityEngine;

namespace LAMENT
{
    public class MonsterTargetPlate : Entity
    {
        private BehaviorTree bt;

        [Header("AI")]
        public Transform target;
        public float detectionRadius;
        public float moveSpeed = 2;

        public float attackDelay = 1;
        public float attackRadius;
        private float lastAttackTime = 0;

        public LayerMask terrainLayer;

        private bool isAttacking = false;
        public Skill skill;

        [Header("Item")]
        public GameObject dropitem;


        protected override void Awake()
        {
            base.Awake();
            BuildBT();
        }

        protected override void Update()
        {
            base.Update();
            bt.Run();

            Animator.SetFloat("HSpeedMagnitude", Math.Abs(MoveComponent.HSpeed));
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

            if (isAttacking)
                return EBTState.SUCCESS;

            if (Vector3.Distance(transform.position, target.position) <= attackRadius)
                return EBTState.SUCCESS;

            return EBTState.FAILURE;
        }

        private EBTState Attack()
        {
            MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);

            if (isAttacking)
                return EBTState.RUN;

            isAttacking = true;
            StartSkill(skill, () => { isAttacking = false; });

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
            {
                MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
                return EBTState.FAILURE;
            }

            // 타겟 추적
            if (target.position.x < transform.position.x)
                MoveComponent.SetMovement(MoveComponent.EMoveState.LEFT);
            else
                MoveComponent.SetMovement(MoveComponent.EMoveState.RIGHT);

            Vector2 dir = Vector2.right * (MoveComponent.MoveState == MoveComponent.EMoveState.LEFT ? -1 : 1);
            // 옆에 벽이 있다면 점프
            if (Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0, dir, 1, terrainLayer))
                MoveComponent.TryJump();

            // 옆 + 아래에 빈 칸이 있다면 점프
            if (!Physics2D.OverlapBox(((Vector2)transform.position) + dir + Vector2.down, Vector2.one * 0.5f, 0, terrainLayer))
                MoveComponent.TryJump();

            return EBTState.RUN;
        }

        public override void OnDamageHandled(Entity src)
        {
            Instantiate(dropitem, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}