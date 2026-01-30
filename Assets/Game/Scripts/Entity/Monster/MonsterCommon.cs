using PLibrary;
using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public class MonsterCommon : Entity
    {
        private BehaviorTree bt;

        [Header("AI - 추적")]
        [SerializeField] private Transform target; // 추적 대상
        [SerializeField] private float detectionRadius = 5; // 추적 반경
        [SerializeField] private float moveSpeed = 2; // 이동 속도

        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private float wallCheckDistance = 1;

        [Header("AI - 공격")]
        [SerializeField] private float attackDelay = 1; // 공격 딜레이
        private float lastAttackTime = 0;
        [SerializeField] private float attackRadius; // 공격 실행 반경
        [SerializeField] private Skill skill;
        private bool isAttacking = false;

        [Header("Item")]
        [SerializeField] private GameObject dropitem;


        protected override void Awake()
        {
            base.Awake();
            BuildBT();
        }

        protected override void Update()
        {
            base.Update();
            bt.Run();

            //Animator.SetFloat("HSpeedMagnitude", Math.Abs(MoveComponent.HSpeed));
        }


        /// <summary> BT 구성 </summary>
        private void BuildBT()
        {
            bt = new();

            BTSelectorNode root = new();
            bt.SetRootNode(root);

            // ===== 추적 시퀀스 =====

            BTSequenceNode seqChase = new();
            root.AddChild(seqChase);

            BTActionNode actCheckChaseRadius = new(CheckChaseRadius);
            seqChase.AddChild(actCheckChaseRadius);

            BTActionNode actCheckObstacle = new(CheckObstacle);
            seqChase.AddChild(actCheckObstacle);

            BTActionNode actChase = new(Chase);
            seqChase.AddChild(actChase);

            // ===== 대기 액션 =====

            BTActionNode actIdle = new(Idle);
            root.AddChild(actIdle);
        }
        private EBTState CheckChaseRadius()
        {
            if (!target)
                return EBTState.FAILURE;

            if (Vector3.Distance(transform.position, target.position) <= detectionRadius)
                return EBTState.SUCCESS;

            return EBTState.FAILURE;
        }

        private EBTState CheckObstacle()
        {
            Vector2 dir = Vector2.right * (target.position.x < transform.position.x ? -1 : 1);

            // 옆에 벽이 있다면 실패
            if (Physics2D.BoxCast(transform.position, Vector2.one * 0.8f, 0, dir, wallCheckDistance, terrainLayer))
                return EBTState.FAILURE;

            // 옆 + 아래에 빈 칸이 있다면 실패
            if (!Physics2D.OverlapBox(((Vector2)transform.position) + dir + Vector2.down, Vector2.one * 0.5f, 0, terrainLayer))
                return EBTState.FAILURE;

            return EBTState.SUCCESS;
        }

        private EBTState Chase()
        {
            if (!target)
                return EBTState.FAILURE;

            // 타겟 추적
            if (target.position.x < transform.position.x)
                MoveComponent.SetMovement(MoveComponent.EMoveState.LEFT);
            else
                MoveComponent.SetMovement(MoveComponent.EMoveState.RIGHT);

            return EBTState.RUN;
        }

        private EBTState Idle()
        {
            MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
            return EBTState.RUN;
        }

        public override void OnDamageHandled(Entity src)
        {
            Instantiate(dropitem, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}