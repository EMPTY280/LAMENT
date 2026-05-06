using UnityEngine;

namespace LAMENT
{
    public abstract class Skill : ScriptableObject
    {
        /// <summary> Comment for inspector display </summary>
        [HideInInspector]
        public abstract string Comment { get; }

        [Header("기본")]
        [SerializeField]
        private float damage;

        [SerializeField, Tooltip("저지 불가, 시전 중에 공격받아도 스킬이 취소되지 않음")]
        private bool isUnstoppable = false;

        public float Damage => damage;

        [Header("Sound")]
        [SerializeField, Tooltip("스킬 사용 성공 시 1회 재생할 SoundData ID")]
        private string soundId = "";

        public string SoundId => soundId;

        [Header("타이밍")]
        [SerializeField, Tooltip("지속 시간 (초), 배정된 애니메이션과 같은 길이로 맞출것.")]
        private float duration = 1.0f;

        [SerializeField, Tooltip("플레이어의 공격 준비 스프라이트가 재생될 시간 (초).")]
        private float playerDelay = 0.1f;

        public float PlayerDelay => playerDelay;
        public float Duration => duration;

        [Header("애니메이션 - 애니메이터")]
        [SerializeField, Tooltip("스킬 시전 시 발동할 애니메이션 트리거.")]
        private string triggerName = "";

        public string TriggerName => triggerName;

        [Header("이펙터 프리팹")]
        [SerializeField, Tooltip("이펙터가 포함된 프리팹을 등록.")]
        private GameObject effector;

        public GameObject Effector => effector;

        [Header("타이밍")]
        [SerializeField, Tooltip("타이밍 리스트, 스킬의 각 효과가 어느 타이밍마다 발생할지의 값, (0 ~ 1)")]
        private float[] timingList = null;

        private int timingPointer = 0;
        private float timeCurr = 0;
        private bool bAdvancePointer = false;

        public void ResetState()
        {
            timingPointer = 0;
            timeCurr = 0;
            bAdvancePointer = false;
        }

        public void OnTiming(float t, Entity owner)
        {
            timeCurr = t;

            Perform(owner);

            if (bAdvancePointer)
            {
                timingPointer++;
                bAdvancePointer = false;
            }
        }

        protected abstract void Perform(Entity owner);

        protected bool IsTiming(int idx)
        {
            if (timingList == null)
                return false;

            if (timingList.Length <= idx)
                return false;

            if (timingPointer == idx && timingList[idx] <= timeCurr)
            {
                bAdvancePointer = true;
                return true;
            }

            return false;
        }

        protected SkillEffector GetEffector(Entity owner)
        {
            if (!owner)
            {
                GameManager.Logger.LogError("스킬의 owner를 찾을 수 없습니다.");
                return null;
            }

            if (!effector)
            {
                GameManager.Logger.LogError("스킬에 배정된 이펙터가 없습니다.");
                return null;
            }

            if (!owner.HasEffector(effector.name))
            {
                GameManager.Logger.LogError("스킬의 이펙터를 찾을 수 없습니다.");
                return null;
            }

            return owner.GetEffector(effector.name);
        }
    }
}