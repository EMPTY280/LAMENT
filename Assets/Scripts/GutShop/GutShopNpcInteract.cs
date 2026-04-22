using UnityEngine;

namespace LAMENT
{
    public class GutShopNpcInteract : MonoBehaviour
    {
        [Header("상점 포탈")]
        [SerializeField] private GutShopNpcPortal portal;

        private bool isPlayerInRange = false;

        private void Update()
        {
            if (!isPlayerInRange)
                return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (portal != null)
                    portal.EnterGutShop();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                isPlayerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                isPlayerInRange = false;
        }
    }
}