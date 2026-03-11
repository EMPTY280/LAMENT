using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public abstract class Entity : MonoBehaviour, IHittable
    {
        // ===== 일반 =====
        [Header("일반")]
        [SerializeField] protected float hpMax = 1;
        [SerializeField] protected float hpCurr = 1;

        protected bool isDead = false; // 사망 여부

        // ===== 애니메이션 =====
        private Animator animator;
        public Animator Animator => animator;

        // ===== 이동 =====
        [Header("이동"), SerializeField]
        private MoveComponent moveComponent;
        public MoveComponent MoveComponent => moveComponent;

        // ===== 스킬 =====

        [Header("스킬"), SerializeField]
        private Transform effectorRoot;
        private Dictionary<string, SkillEffector> effectors = new(); // 장비 이펙터 캐시
        private Skill skillCurr = null; // 현재 시전중인 스킬 (공격)
        private float skillDurationCurr = 0;
        private Action cbOnSkillEnd;


        protected virtual void Awake()
        {
            TryGetComponent(out animator);
        }

        protected virtual void Update()
        {
            UpdateSkill();
        }

        protected virtual void OnDestroy()
        {
            effectors.Clear();
        }

        #region 이펙터

        /// <summary> 그 이름의 이펙터가 존재하는가? </summary>
        public bool HasEffector(string name)
        {
            return effectors.ContainsKey(name);
        }

        public SkillEffector GetEffector(string name)
        {
            return effectors[name];
        }

        /// <summary> 그 스킬에 사용되는 이펙터 생성 시도 </summary>
        protected bool TryAddEffector(Skill data)
        {
            if (!data)
            {
                GameManager.Logger.LogError("null 스킬에서 이펙터를 생성할 수 없습니다.");
                return false;
            }

            // 배정된 이펙터가 없다면 패스 (유틸리티 스킬 등)
            if (!data.Effector)
                return false;

            // 중복된 이펙터 검사
            if (HasEffector(data.Effector.name))
                return false;

            if (Instantiate(data.Effector, effectorRoot ?? transform).TryGetComponent(out SkillEffector eff))
            {
                eff.SetOwner(this);
                eff.transform.localPosition = Vector3.zero;
                eff.CB_OnHitTarget = OnHitTarget;

                effectors.Add(data.Effector.name, eff);
                return true;
            }
            else
            {
                GameManager.Logger.LogError("스킬 이펙터 프리팹에서 스크립트를 찾을 수 없습니다.");
                return false;
            }
        }

        #endregion

        #region 스킬

        /// <summary> 실행중인 스킬이 있다면 타이밍 업데이트 및 종료 판정 </summary>
        private void UpdateSkill()
        {
            if (skillCurr == null)
                return;

            skillDurationCurr += Time.deltaTime;
            skillCurr.OnTiming(skillDurationCurr / skillCurr.Duration, this);

            if (skillCurr.Duration <= skillDurationCurr)
            {
                skillCurr = null;
                if (animator)
                    animator.SetBool("UsingSkill", false);
                if (cbOnSkillEnd != null)
                    cbOnSkillEnd();
            }
        }

        /// <summary> 스킬 사용 시도 </summary>
        public bool TryStartSkill(Skill skill, Action cbOnSkillEnd = null)
        {
            if (skill == null)
                return false;

            skill.ResetState();
            skillCurr = skill;
            skillDurationCurr = 0;

            if (animator && skill.TriggerName != "")
            {
                animator.SetBool("UsingSkill", true);
                animator.SetTrigger(skill.TriggerName);
            }

            this.cbOnSkillEnd = cbOnSkillEnd;
            return true;
        }

        #endregion

        #region 체력, 피격 및 사망

        /// <summary> 피격 당했을 때 호출, 유효한 타격만 받음 타격 성공 여부 반환 </summary>
        public bool OnHitTaken(DamageResponse rsp)
        {
            TakeDamage(rsp);
            return true;
        }

        /// <summary> 피해 받기 계산 </summary>
        protected virtual void TakeDamage(DamageResponse rsp) { }

        /// <summary> 체력 설정 </summary>
        public virtual void SetHP(float amount, bool isRelative)
        {
            if (isRelative)
                hpCurr = math.min(hpCurr + amount, hpMax);
            else
                hpCurr = math.min(amount, hpMax);
        }

        /// <summary> 사망 시 호출 </summary>
        protected virtual void OnDied() {}

        #endregion
    
        #region 타격

        /// <summary> 대상 타격 시 호출 </summary>
        private void OnHitTarget(IHittable target)
        {
            target.OnHitTaken(new()
            {
                src = this,
                amount = skillCurr.Damage
            });

            OnHitTarget(target, skillCurr);
        }

        /// <summary> 타격 후 추가 효과 </summary>
        protected virtual void OnHitTarget(IHittable target, Skill skill) {}

        #endregion
    }
}