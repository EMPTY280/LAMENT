using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public class PlayerIndocator : MonoBehaviour
    {
        [Header("HP"), SerializeField]
        private HPIcon[] hpIcons;
        [SerializeField] private float hpAnimSpeedStable = 5f; // 최대 체력일 때 애니 속도
        [SerializeField] private float hpAnimSpeedLethal = 11f; // 최소 체력일 때 애니 속도


        private void Awake()
        {
            // 이벤트 등록
            GameManager.Eventbus.Subscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
        }

        private void OnDestroy()
        {
            // 이벤트 삭제
            GameManager.Eventbus.Unsubscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
        }

        #region HP

        private void OnPlayerHealthChanged(GEOnPlayerHealthChanged e)
        {
            // 초기화 여부 확인
            if (hpIcons == null || hpIcons.Length == 0)
                return;

            // 애니 속도 계산
            float animSpeed = math.lerp(hpAnimSpeedLethal, hpAnimSpeedStable, (float)e.CurrentHp / e.CurrentMaxHp);

            // 모든 아이콘을 순회
            for (int i = 0; i < hpIcons.Length; i++)
            {
                HPIcon ele = hpIcons[i];

                bool isVisible = i < e.InitialMaxHp;
                bool isFilled = i < e.CurrentHp;

                ele.SetVisible(isVisible);
                ele.SetFilled(isFilled);
                ele.SetAnimSpeed(animSpeed);
            }
        }

        #endregion
    
        #region 에너지



        #endregion
    }
}