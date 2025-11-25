using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class StomachBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public void SetFill(float ratio)
        {
            if (fillImage == null) return;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}