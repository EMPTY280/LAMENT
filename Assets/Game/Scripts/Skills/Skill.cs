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
        [Tooltip("지속 시간 (초), 배정된 애니메이션과 같은 길이로 맞출것.")]
        private float duration = 1.0f;
        [SerializeField]
        [Tooltip("스킬 시전 시 발동할 애니메이션 트리거.")]
        private string triggerName = "";
        [SerializeField]
        [Tooltip("저지 불가, 시전 중에 공격받아도 스킬이 취소되지 않음")]
        private bool isUnstoppable = false;

        public float Duration => duration;
        public string TriggerName => triggerName;

        [Header("이펙터 프리팹")]
        [SerializeField]
        [Tooltip("이펙터 가 포함된 프리팹을 등록.")]
        private GameObject effector;

        public GameObject Effector => effector;

        [Header("타이밍")]
        [SerializeField]
        [Tooltip("타이밍 리스트, 스킬의 각 효과가 어느 타이밍마다 발생할지의 값, (0 ~ 1)")]
        private float[] timingList = null;
        private int timingPointer = 0; // 현재 대기중인 타이밍 인덱스
        private float timeCurr = 0; // 현재 스킬 진행도
        private bool bAdvancePointer = false; // 타이밍 포인터를 전진할지 여부


        /// <summary> 진행 상태 초기화 </summary>
        public void ResetState()
        {
            timingPointer = 0;
            timeCurr = 0;
            bAdvancePointer = false;
        }

        /// <summary> 스킬 진행도에 따라 호출 </summary>
        /// <param name="t"> 정규화된 시간, 0 ~ 1 </param>
        /// <param name="owner"> 스킬의 사용자 </param>
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

        /// <summary> IsTiming() 메서드로 각 타이밍에 실행할 행동을 구현 </summary>
        protected abstract void Perform(Entity owner);

        /// <summary> 그 인덱스의 타이밍을 실행할 때가 맞는지 반환, true면 포인터 1 전진 </summary>
        protected bool IsTiming(int idx)
        {
            if (timingList == null)
                return false;

            if (timingList.Length <= idx)
                return false;

            // 포인터와 인덱스가 동일하고, 진행도가 충분히 지났다면
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

            SkillEffector rst;
            if (!owner.Effectors.TryGetValue(effector.name, out rst))
                GameManager.Logger.LogError("스킬의 이펙터를 찾을 수 없습니다.");

            return rst;
        }
    }
}