using System;
using UnityEngine;
using UnityEngine.UI;


namespace LAMENT
{
    public class ScreenFade : MonoBehaviour
    {
        [SerializeField] private Image img;

        private bool isActive = false;
        private bool isFadeout = true;
        private float speed = 1f;

        private Action cbOnFinished;


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (!isActive)
                return;

            Color colorNext = img.color;
            colorNext.a += (isFadeout ? speed : -speed) * Time.deltaTime;
            img.color = colorNext;

            if ((isFadeout && 1 <= colorNext.a) ||
                (!isFadeout && colorNext.a <= 0))
            {
                isActive = false;
                img.raycastTarget = false;
                if (cbOnFinished != null)
                    cbOnFinished();
            }
        }

        public bool TryStartFadeout(float duration, Action onFinished = null)
        {
            if (isActive)
                return false;
            
            isActive = true;
            isFadeout = true;
            SetSpeed(duration);

            cbOnFinished = onFinished;

            img.raycastTarget = true;
            img.color = Color.clear;

            return true;
        }

        public bool TryStartFadein(float duration, Action onFinished = null)
        {
            if (isActive)
                return false;
            
            isActive = true;
            isFadeout = false;
            SetSpeed(duration);

            cbOnFinished = onFinished;

            img.raycastTarget = true;
            img.color = Color.black;

            return true;
        }

        private void SetSpeed(float duration)
        {
            if (duration <= 0)
            {
                GameManager.Logger.LogError("페이딩 지속 시간을 0초 이하로 설정할 수 없습니다.");
                return;
            }

            speed = 1 / duration;
        }
    }
}