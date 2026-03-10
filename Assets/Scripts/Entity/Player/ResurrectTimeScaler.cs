using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class ResurrectTimeScaler : MonoBehaviour
    {
        private bool isActive = false;

        [SerializeField] private Image overlayImg;
        [SerializeField] private float duration = 1.0f;
        private float timeScalePrev = 1f;
        private float timeScaleRestoreSpeed = 0;


        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnPlayerResurrected>(OnPlayerResurrected);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnPlayerResurrected>(OnPlayerResurrected);
        }

        private void Update()
        {
            if (!isActive)
                return;

            Time.timeScale += Time.unscaledDeltaTime * timeScaleRestoreSpeed;

            Color newColor = overlayImg.color;
            newColor.a = (1 - (Time.timeScale / timeScalePrev)) * 0.4f;
            overlayImg.color = newColor;

            if (timeScalePrev <= Time.timeScale)
            {
                Time.timeScale = timeScalePrev;
                isActive = false;
            }
        }

        
        private void OnPlayerResurrected(GEOnPlayerResurrected e)
        {
            if (isActive)
                return;
            isActive = true;

            timeScalePrev = Time.timeScale;
            timeScaleRestoreSpeed = timeScalePrev / duration;

            Time.timeScale = 0f;
        }
    }
    
}
