using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class HPIcon : MonoBehaviour
    {
        [Header("외형")]
        [SerializeField] private Image Img;
        [SerializeField, Space(5)] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;

        // ===== 애니 =====
        private bool isFilled = true;
        private float animSpeed = 1.2f;


        #region 업데이트

        private void Update()
        {
            UpdateAnim();
        }

        private void UpdateAnim()
        {
            if (!isFilled)
                return;

            // 0 ~ 1 사이 반복
            float animTime = (math.sin(Time.time * animSpeed) + 1) * 0.5f;

            transform.localScale = new Vector3(
                1f - 0.1f * animTime,
                0.9f + 0.1f * animTime,
                1f
            );
        }

        #endregion

        #region 외형

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetFilled(bool isFilled)
        {
            this.isFilled = isFilled;
            Img.sprite = isFilled ? fullSprite : emptySprite;

            if (!isFilled)
                transform.localScale = Vector3.one;
        }

        public void SetAnimSpeed(float spd)
        {
            animSpeed = spd;
        }

        #endregion
    }
}