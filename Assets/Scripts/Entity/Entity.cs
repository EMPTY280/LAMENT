using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public abstract class Entity : MonoBehaviour, IHittable
    {
        [Header("일반")]
        [SerializeField] protected float hpMax = 1;
        [SerializeField] protected float hpCurr = 1;

        protected bool isDead = false;

        private Animator animator;
        public Animator Animator => animator;

        [Header("이동"), SerializeField]
        private MoveComponent moveComponent;
        public MoveComponent MoveComponent => moveComponent;

        [Header("스킬"), SerializeField]
        private Transform effectorRoot;
        private Dictionary<string, SkillEffector> effectors = new();
        private Skill skillCurr = null;
        private float skillDurationCurr = 0;
        private Action cbOnSkillEnd;

        private float skillDamageMultiplierCurr = 1f;

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

        public bool HasEffector(string name)
        {
            return effectors.ContainsKey(name);
        }

        public SkillEffector GetEffector(string name)
        {
            return effectors[name];
        }

        protected bool TryAddEffector(Skill data)
        {
            if (!data)
            {
                GameManager.Logger.LogError("null 스킬에서 이펙터를 생성할 수 없습니다.");
                return false;
            }

            if (!data.Effector)
                return false;

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

        private void UpdateSkill()
        {
            if (skillCurr == null)
                return;

            skillDurationCurr += Time.deltaTime;
            skillCurr.OnTiming(skillDurationCurr / skillCurr.Duration, this);

            if (skillCurr.Duration <= skillDurationCurr)
            {
                skillCurr = null;
                skillDamageMultiplierCurr = 1f;

                if (animator)
                    animator.SetBool("UsingSkill", false);

                if (cbOnSkillEnd != null)
                    cbOnSkillEnd();
            }
        }

        public bool TryStartSkill(Skill skill, Action cbOnSkillEnd = null, float damageMultiplier = 1f)
        {
            if (skill == null)
                return false;

            skill.ResetState();
            skillCurr = skill;
            skillDurationCurr = 0;
            skillDamageMultiplierCurr = Mathf.Max(0f, damageMultiplier);

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

        public bool OnHitTaken(DamageResponse rsp)
        {
            TakeDamage(rsp);
            return true;
        }

        protected virtual void TakeDamage(DamageResponse rsp) { }

        public virtual void SetHP(float amount, bool isRelative)
        {
            if (isRelative)
                hpCurr = math.min(hpCurr + amount, hpMax);
            else
                hpCurr = math.min(amount, hpMax);
        }

        protected virtual void OnDied() {}

        #endregion
    
        #region 타격

        private void OnHitTarget(IHittable target)
        {
            float damage = skillCurr != null ? skillCurr.Damage * skillDamageMultiplierCurr : 0f;

            target.OnHitTaken(new()
            {
                src = this,
                amount = damage
            });

            OnHitTarget(target, skillCurr);
        }

        protected virtual void OnHitTarget(IHittable target, Skill skill) {}

        #endregion
    }
}