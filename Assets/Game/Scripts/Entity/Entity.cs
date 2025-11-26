using System;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public abstract class Entity : MonoBehaviour, IDamageable
    {
        private Animator animator;
        public Animator Animator => animator;

        [Header("일반")]
        [SerializeField]
        protected float hpMax = 1;
        protected float hpCurr = 1;

        protected bool isInvulnerable = false; // 무적
        protected bool isUnstoppable = false;  // 저지불가

        [Header("이동")]
        [SerializeField] private MoveComponent moveComponent;
        public MoveComponent MoveComponent => moveComponent;

        // ===== 스킬 =====
        protected Skill skillCurr = null; // 현재 시전중인 스킬 (공격)
        protected float skillDurationCurr = 0;
        protected Action cbOnSkillEnd;

        private Dictionary<string, SkillEffector> effectors = new(); // 장비 이펙터 모음
                                                               // NOTE: 장비 교체가 잦은 게임이므로, 장비가 파괴되어도 그대로 유지함 (On / Off)
        public Dictionary<string, SkillEffector> Effectors => effectors;


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
                animator.SetBool("UsingSkill", false);
                if (cbOnSkillEnd != null)
                    cbOnSkillEnd();
            }
        }

        public void StartSkill(Skill skill, Action cbOnSkillEnd = null)
        {
            if (skill == null)
                return;

            skill.ResetState();
            skillCurr = skill;
            skillDurationCurr = 0;

            if (animator && skill.TriggerName != "")
            {
                animator.SetBool("UsingSkill", true);
                animator.SetTrigger(skill.TriggerName);
            }

            this.cbOnSkillEnd = cbOnSkillEnd;
        }

        #endregion

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IDamageable target))
            {
                Debug.Log(collision.name);
                OnHitTarget(target);
                target.OnDamaged(this);
            }
        }

        /// <summary> 타격 가능한 대상 타격 시 호출 </summary>
        public virtual void OnHitTarget(IDamageable target)
        {
            Debug.Log("I HIT SOME ASS!");
        }

        /// <summary> 타격당했을 때 호출 </summary>
        public virtual void OnDamaged(Entity src)
        {
            Debug.Log("SOMEONE HIT MY ASS!");
        }
    }
}