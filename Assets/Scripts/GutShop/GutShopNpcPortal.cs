using UnityEngine;

namespace LAMENT
{
    public class GutShopNpcPortal : MonoBehaviour
    {
        [Header("상점 씬 이름")]
        [SerializeField] private string gutShopSceneName = "GutShop";

        /// <summary>
        /// NPC 상호작용 시스템에서 호출할 진입점
        /// </summary>
        public void EnterGutShop()
        {
            GameManager.Instance.TryOpenOverlayScene(gutShopSceneName);
        }
    }
}