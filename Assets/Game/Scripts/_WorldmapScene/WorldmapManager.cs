using UnityEngine;

namespace LAMENT
{
    public class WorldmapManager : MonoBehaviour
    {
        public void OnCoffinButtonClicked()
        {
            GameManager.Instance.TryChangeScene("Collections");
        }
    }
}