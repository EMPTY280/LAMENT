using System;
using PLibrary;
using UnityEngine;


namespace LAMENT
{
    public class MonsterSpiderBoss2 : Entity
    {
        private BehaviorTree bt;

        // ===== AI =====
        [Header("AI"), SerializeField] private Transform target;

        private Action currAttack = null; // FSM을 따로 만들지 않고 간단 구현.
        private int attackState = 0;

        // ===== 돌진 =====
        private float lastChargeTime = 0f;
        private float chargeHitTime = 0f;
        [Header("AI - 돌진"), SerializeField] private float chargeInterval = 3f;
        [SerializeField] private float chargeRecoverDelay = 0.5f;
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private float wallCheckDistance = 1;

        // ===== 발사체 =====

        private float lastSpitTime = 0f;
        [Header("AI - 발사체"), SerializeField] private float spitInterval = 1.2f;
        [SerializeField] private float spitDelayBefore = 0.7f;
        [SerializeField] private float spitDelayAfter = 0.7f;
        [SerializeField] private float spitSpeed = 15f;
        [SerializeField] private Projectile projectile;
        [SerializeField] private Transform spitPos;


        protected override void Awake()
        {
            base.Awake();
            BuildBT();
        }

        protected override void Update()
        {
            base.Update();

            // 현재 실행중인 패턴이 없을 때만 BT 실행
            if (currAttack == null)
                bt.Run();
            else
                currAttack();
        }

        /// <summary> BT 구성 및 AI </summary>
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

            BTActionNode actStartCharging = new(StartCharging);
            seqCharge.AddChild(actStartCharging);

            // ===== 발사체 시퀀스 =====
            BTSequenceNode seqSpit = new();
            root.AddChild(seqSpit);

            BTActionNode actCheckSpit = new(CheckSpit);
            seqSpit.AddChild(actCheckSpit);

            BTActionNode actStartSpitting = new(StartSpitting);
            seqSpit.AddChild(actStartSpitting);

            // ===== 정지 =====
            BTActionNode actIdle = new(Idle);
            root.AddChild(actIdle);
        }

        private void ChangeAttackState(Action a)
        {
            currAttack = a;
            attackState = 0;
        }

        #region 돌진

        private EBTState CheckCharge()
        {
            return (lastChargeTime + chargeInterval < Time.time) ? EBTState.SUCCESS : EBTState.FAILURE;
        }

        private EBTState StartCharging()
        {
            ChangeAttackState(ChargeAttack);
            return EBTState.SUCCESS;
        }

        private void ChargeAttack()
        {
            switch (attackState)
            {
                case 0: // 돌진 방향 지정
                    if (target.position.x < transform.position.x)
                        MoveComponent.SetMovement(MoveComponent.EDirection.LEFT);
                    else
                        MoveComponent.SetMovement(MoveComponent.EDirection.RIGHT);
                    attackState = 1;
                break;

                case 1: // 돌진 중, 충돌 판정
                {
                    Vector2 dir = MoveComponent.Direction == MoveComponent.EDirection.LEFT ? Vector2.left : Vector2.right;
                    if (Physics2D.BoxCast(transform.position, Vector2.one * 0.8f, 0, dir, wallCheckDistance, terrainLayer))
                    {
                        // 돌진 회복으로 전환
                        attackState = 2;
                        chargeHitTime = Time.time;
                    }
                }
                break;

                case 2: // 돌진 회복
                    if (chargeHitTime + chargeRecoverDelay <= Time.time)
                    {
                        lastChargeTime = Time.time; // 쿨다운 적용
                        ChangeAttackState(null);
                    }
                break;
            }
        }

        #endregion

        #region 발사체

        private EBTState CheckSpit()
        {
            return (lastSpitTime + spitInterval < Time.time) ? EBTState.SUCCESS : EBTState.FAILURE;
        }

        private EBTState StartSpitting()
        {
            lastSpitTime = Time.time;
            ChangeAttackState(SpitAttack);
            return EBTState.SUCCESS;
        }

        private void SpitAttack()
        {
            switch (attackState)
            {
                case 0: // 선딜레이 및 발사
                    MoveComponent.SetDirection(target.position.x < transform.position.x ? MoveComponent.EDirection.LEFT : MoveComponent.EDirection.RIGHT);

                    if (lastSpitTime + spitDelayBefore <= Time.time)
                    {
                        // 발사
                        if (target.position.x < transform.position.x)
                            projectile.Fire(spitPos.transform.position, Vector2.left * spitSpeed, OnSpitHit);
                        else
                            projectile.Fire(spitPos.transform.position, Vector2.right * spitSpeed, OnSpitHit);


                        attackState = 1;
                        lastSpitTime = Time.time;
                    }
                break;

                case 1: // 후딜레이 및 종료
                    if (lastSpitTime + spitDelayAfter <= Time.time)
                    {
                        lastSpitTime = Time.time;
                        ChangeAttackState(null);
                    }
                break;
            }
        }

        private void OnSpitHit(Projectile p, Collider2D col)
        {
            switch (col.gameObject.layer)
            {
                case 6: // Hittable
                    if (col.transform.parent.name == this.name)
                        return;
                break;

                case 3: // Terrain
                break;
            }

            p.SetActive(false);
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