using PLibrary;
using UnityEngine;


namespace LAMENT
{
    public class MonsterSpiderBoss2 : Entity
    {
        private BehaviorTree bt;

        // ===== AI =====
        [Header("AI"), SerializeField] private Transform target;

        // ===== 돌진 =====
        private bool isCharging = false;
        private float lastChargeTime = 0f;
        [Header("AI - 돌진"), SerializeField] private float chargeDelay = 2f;
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private float wallCheckDistance = 1;


        protected override void Awake()
        {
            base.Awake();
            BuildBT();
        }

        protected override void Update()
        {
            base.Update();
            bt.Run();
        }

        /// <summary> BT 구성 </summary>
        private void BuildBT()
        {
            bt = new();

            BTSelectorNode root = new();
            bt.SetRootNode(root);

            // ===== 근접 - 발사체 연계 시퀀스 =====

            // ===== 근접 공격 시퀀스 =====
            
            // ===== 돌진 시퀀스 =====
            BTSequenceNode seqCharge = new();
            root.AddChild(seqCharge);

            BTActionNode actCheckCharge = new(CheckCharge);
            seqCharge.AddChild(actCheckCharge);

            BTActionNode actProcCharge = new(ProcCharge);
            seqCharge.AddChild(actProcCharge);

            // ===== 발사체 시퀀스 =====

            // ===== 정지 =====
            BTActionNode actIdle = new(Idle);
            root.AddChild(actIdle);
        }

        #region 돌진

        private EBTState CheckCharge()
        {
            return (lastChargeTime + chargeDelay < Time.time) ? EBTState.SUCCESS : EBTState.FAILURE;
        }

        private EBTState ProcCharge()
        {
            if (!isCharging)
            {
                // 돌진 방향 지정
                if (target.position.x < transform.position.x)
                    MoveComponent.SetMovement(MoveComponent.EDirection.LEFT);
                else
                    MoveComponent.SetMovement(MoveComponent.EDirection.RIGHT);

                isCharging = true;
                return EBTState.RUN;
            }

            // 벽 충돌 판정
            Vector2 dir = MoveComponent.Direction == MoveComponent.EDirection.LEFT ? Vector2.left : Vector2.right;
            if (Physics2D.BoxCast(transform.position, Vector2.one * 0.8f, 0, dir, wallCheckDistance, terrainLayer))
            {
                // 쿨다운 적용
                lastChargeTime = Time.time;

                // 돌진 종료
                isCharging = false;
            }

            return EBTState.RUN;
        }

        #endregion

        #region 정지

        private EBTState Idle()
        {
            MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
            
            if (target.position.x < transform.position.x)
                MoveComponent.SetDirection(MoveComponent.EDirection.LEFT);
            else
                MoveComponent.SetDirection(MoveComponent.EDirection.RIGHT);

            return EBTState.RUN;
        }

        #endregion
    }
}