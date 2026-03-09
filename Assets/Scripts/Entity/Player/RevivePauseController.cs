using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class RevivePauseController : MonoBehaviour
    {
        [Header("부활 후 게임 딜레이 (초)")]
        [SerializeField] private float revivePauseDuration = 1.0f;

        private float _prevTimeScale = 1f;
        private Coroutine _pauseRoutine;

        public static bool IsRevivePaused { get; private set; }

        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnPlayerRevived>(OnPlayerRevived);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnPlayerRevived>(OnPlayerRevived);
        }

        private void OnPlayerRevived(GEOnPlayerRevived e)
        {
            if (_pauseRoutine != null)
                StopCoroutine(_pauseRoutine);

            _pauseRoutine = StartCoroutine(PauseRoutine());
        }

        private IEnumerator PauseRoutine()
        {
            IsRevivePaused = true;

            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f; // 게임 전체 정지

            // timeScale=0 상태에서도 기다리기 위해 Realtime 사용
            yield return new WaitForSecondsRealtime(revivePauseDuration);

            Time.timeScale = _prevTimeScale;
            IsRevivePaused = false;
            _pauseRoutine = null;
        }
    }
    
}
