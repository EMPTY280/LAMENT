using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class HPIcon : MonoBehaviour
    {
        [Header("외형")]
        [SerializeField] private Image img;
        [SerializeField, Space(5)] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;

        // ===== 애니 =====
        private bool isFilled = true;
        private float animSpeed = 1.2f;

        [Header("애니메이션")]
        [SerializeField] private float throbAnimSpeed = 1f; // 최대 체력 복구 시 커졌다가 다시 작아지는 애니
        private float throbScale = 1.0f;


        #region 업데이트

        private void Update()
        {
            UpdateAnim();
        }

        private void UpdateAnim()
        {
            if (1.0f < throbScale)
            {
                throbScale -= Time.deltaTime * throbAnimSpeed;
                if (throbScale <= 1.0f)
                    throbScale = 1.0f;

                if (!isFilled)
                    transform.localScale = new Vector3(throbScale, throbScale, 0f);
            }

            if (isFilled)
            {
                // 0 ~ 0.5 사이 반복
                float animTime = (math.sin(Time.time * animSpeed) + 1) * 0.5f;

                transform.localScale = new Vector3(
                    (1f - 0.1f * animTime) * throbScale,
                    (0.9f + 0.1f * animTime) * throbScale,
                    1f
                ); 
            }
        }

        #endregion

        #region 외형

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetFaded(bool isFaded)
        {
            img.color = isFaded ? Color.black : Color.white;

            if (isFaded)
                transform.localScale = Vector3.one;
        }

        public void SetFilled(bool isFilled)
        {
            this.isFilled = isFilled;
            img.sprite = isFilled ? fullSprite : emptySprite;

            if (!isFilled)
                transform.localScale = Vector3.one;
        }

        public void SetAnimSpeed(float spd)
        {
            animSpeed = spd;
        }

        public void SetThrobScale(float v)
        {
            throbScale = v;
        }

        #endregion
    }
}