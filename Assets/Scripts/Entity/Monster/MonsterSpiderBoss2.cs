using System;
using PLibrary;
using Unity.Mathematics;
using UnityEngine;


namespace LAMENT
{
    public class MonsterSpiderBoss2 : Monster
    {
        private BehaviorTree bt;
        private Action currAttack = null; // FSM을 따로 만들지 않고 간단 구현.
        private int attackState = 0;

        // ===== 연계 =====
        [Header("AI - 발사체 연계")]
        [SerializeField] private float comboRecoverDelay = 0.5f;
        private float lastComboTime = 0f;

        // ===== 근접 =====
        [Header("AI - 근접")]
        [SerializeField] private Vector2 knockbackForce;
        [SerializeField] private float meleeRange;
        [SerializeField] private float meleeInterval = 3f;
        private float lastMeleeTime = 0f;
        private bool canStartCombo = false;

        // ===== 돌진 =====
        private float lastChargeTime = 0f;
        private float chargeHitTime = 0f;
        [Header("AI - 돌진")]
        [SerializeField] private float chargeInterval = 3f;
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

            // ===== 발사체 연계 시퀀스 =====
            BTSequenceNode seqCombo = new();
            root.AddChild(seqCombo);

            BTActionNode actCheckCombo = new(CheckCombo);
            seqCombo.AddChild(actCheckCombo);

            BTActionNode actStartCombo = new(StartCombo);
            seqCombo.AddChild(actStartCombo);

            // ===== 근접 공격 시퀀스 =====
            BTSequenceNode seqMelee = new();
            root.AddChild(seqMelee);

            BTActionNode actCheckMelee = new(CheckMelee);
            seqMelee.AddChild(actCheckMelee);

            BTActionNode actStartMelee = new(StartMelee);
            seqMelee.AddChild(actStartMelee);
            
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

        #region 발사체 연계

        private EBTState CheckCombo()
        {
            return canStartCombo ? EBTState.SUCCESS : EBTState.FAILURE;
        }

        private EBTState StartCombo()
        {
            // 발사
            projectile.CB_OnHitTarget = OnSpitHit;
            projectile.Fire(this,
                spitPos.transform.position,
                Vector2.right * spitSpeed * math.sign(target.position.x - transform.position.x)
            );

            lastComboTime = Time.time;
            canStartCombo = false;
            ChangeAttackState(ComboAttack);
            return EBTState.SUCCESS;
        }

        private void ComboAttack()
        {
            if (lastComboTime + comboRecoverDelay < Time.time)
                ChangeAttackState(null);
        }

        #endregion

        #region 근접

        private EBTState CheckMelee()
        {
            if (meleeRange < Vector2.Distance(target.position, transform.position))
                return EBTState.FAILURE;
            return (lastMeleeTime + meleeInterval < Time.time) ? EBTState.SUCCESS : EBTState.FAILURE;
        }

        private EBTState StartMelee()
        {
            lastMeleeTime = Time.time;
            TryStartSkill(skills[0], () => ChangeAttackState(null));

            ChangeAttackState(MeleeAttack);
            return EBTState.SUCCESS;
        }

        private void MeleeAttack()
        {
            
        }

        protected override void OnHitTarget(IHittable target, Skill skill)
        {
            if ((target as TriggerHandler).TryGetOwner(out Entity owner))
            {
                float dir = MoveComponent.Direction == MoveComponent.EDirection.LEFT ? -1 : 1;
                owner.MoveComponent.AddForce(new Vector2(knockbackForce.x * dir, knockbackForce.y));
                canStartCombo = true;
            }
        }

        #endregion

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
                        projectile.CB_OnHitTarget = OnSpitHit;
                        projectile.Fire(this,
                            spitPos.transform.position,
                            Vector2.right * spitSpeed * math.sign(target.position.x - transform.position.x)
                        );

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

        private void OnSpitHit(IHittable target, Collider2D col)
        {
            target.OnHitTaken(new()
            {
                src = this,
                amount = 1
            });
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