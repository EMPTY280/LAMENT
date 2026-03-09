using UnityEngine;



namespace LAMENT
{
    /// <summary> 선택된 위치를 보여주는 커서 </summary>
    public class CursorRect : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!rectTransform)
                return;
                
            float size = Mathf.Sin(Time.time * 10) * 2f;

            rectTransform.offsetMax = Vector2.one * (10 - size); // Right, Top
            rectTransform.offsetMin = -Vector2.one * (10 - size); // Left, Bottom
        }

        /// <summary> 부모를 지정하여 위치 설정 </summary>
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
            Update();
        }
    }
}