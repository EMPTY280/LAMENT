using TMPro;
using UnityEngine;

namespace LAMENT
{
    public sealed class QTEUIPresenter : MonoBehaviour
    {
        [Header("루트")]
        [SerializeField] private GameObject panelRoot;

        [Header("텍스트")]
        [SerializeField] private TMP_Text sequenceText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text resultText;

        [Header("표시")]
        [SerializeField] private float resultShowDuration = 0.5f;

        private bool isRunning = false;
        private bool isShowingResult = false;
        private float startedUnscaledTime = 0f;
        private float timeLimit = 0f;
        private float resultHideTime = 0f;

        private int currentIndex = 0;
        private int totalCount = 0;
        private EQTEDirection[] lastSequence;

        private void Awake()
        {
            Debug.Log("[QTE][UI] AWAKE");
            HideImmediate();
        }

        private void OnEnable()
        {
            Debug.Log("[QTE][UI] ENABLED");

            GameManager.Eventbus.Subscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Subscribe<GEOnQTEProgress>(OnQTEProgress);
            GameManager.Eventbus.Subscribe<GEOnQTESucceeded>(OnQTESucceeded);
            GameManager.Eventbus.Subscribe<GEOnQTEFailed>(OnQTEFailed);
            GameManager.Eventbus.Subscribe<GEOnQTEFinished>(OnQTEFinished);
        }

        private void OnDisable()
        {
            Debug.Log("[QTE][UI] DISABLED");

            GameManager.Eventbus.Unsubscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Unsubscribe<GEOnQTEProgress>(OnQTEProgress);
            GameManager.Eventbus.Unsubscribe<GEOnQTESucceeded>(OnQTESucceeded);
            GameManager.Eventbus.Unsubscribe<GEOnQTEFailed>(OnQTEFailed);
            GameManager.Eventbus.Unsubscribe<GEOnQTEFinished>(OnQTEFinished);
        }

        private void Update()
        {
            if (isRunning)
                UpdateTimer();

            if (isShowingResult && Time.unscaledTime >= resultHideTime)
                HideImmediate();
        }

        private void OnQTEStarted(GEOnQTEStarted e)
        {
            Debug.Log("[QTE][UI] START RECEIVED");

            isRunning = true;
            isShowingResult = false;
            startedUnscaledTime = Time.unscaledTime;
            timeLimit = e.TimeLimit;

            currentIndex = 0;
            totalCount = e.Sequence != null ? e.Sequence.Length : 0;
            lastSequence = e.Sequence;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (sequenceText != null)
                sequenceText.text = BuildSequenceText(lastSequence, currentIndex);

            if (progressText != null)
                progressText.text = $"{currentIndex} / {totalCount}";

            if (timerText != null)
                timerText.text = timeLimit.ToString("F2");

            if (resultText != null)
                resultText.text = string.Empty;
        }

        private void OnQTEProgress(GEOnQTEProgress e)
        {
            Debug.Log($"[QTE][UI] PROGRESS {e.CurrentIndex}/{e.TotalCount}");

            currentIndex = Mathf.Clamp(e.CurrentIndex, 0, e.TotalCount);
            totalCount = Mathf.Max(0, e.TotalCount);

            if (sequenceText != null)
                sequenceText.text = BuildSequenceText(lastSequence, currentIndex);

            if (progressText != null)
                progressText.text = $"{currentIndex} / {totalCount}";
        }

        private void OnQTESucceeded(GEOnQTESucceeded e)
        {
            Debug.Log("[QTE][UI] SUCCESS RECEIVED");

            isRunning = false;

            if (resultText != null)
                resultText.text = "SUCCESS";

            if (timerText != null)
                timerText.text = "0.00";
        }

        private void OnQTEFailed(GEOnQTEFailed e)
        {
            Debug.Log("[QTE][UI] FAIL RECEIVED");

            isRunning = false;

            if (resultText != null)
                resultText.text = "FAIL";

            if (timerText != null)
                timerText.text = "0.00";
        }

        private void OnQTEFinished(GEOnQTEFinished e)
        {
            Debug.Log($"[QTE][UI] FINISHED success:{e.IsSuccess}");

            isRunning = false;
            isShowingResult = true;
            resultHideTime = Time.unscaledTime + resultShowDuration;
        }

        private void UpdateTimer()
        {
            float remain = Mathf.Max(0f, timeLimit - (Time.unscaledTime - startedUnscaledTime));

            if (timerText != null)
                timerText.text = remain.ToString("F2");
        }

        private void HideImmediate()
        {
            isRunning = false;
            isShowingResult = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (sequenceText != null)
                sequenceText.text = string.Empty;

            if (progressText != null)
                progressText.text = string.Empty;

            if (timerText != null)
                timerText.text = string.Empty;

            if (resultText != null)
                resultText.text = string.Empty;
        }

        private string BuildSequenceText(EQTEDirection[] sequence, int solvedCount)
        {
            if (sequence == null || sequence.Length == 0)
                return string.Empty;

            string result = "";

            for (int i = 0; i < sequence.Length; i++)
            {
                string symbol = DirectionToSymbol(sequence[i]);

                if (i < solvedCount)
                    result += $"<s>{symbol}</s>";
                else if (i == solvedCount)
                    result += $"<b>[{symbol}]</b>";
                else
                    result += symbol;

                if (i < sequence.Length - 1)
                    result += "  ";
            }

            return result;
        }

        private string DirectionToSymbol(EQTEDirection dir)
        {
            switch (dir)
            {
                case EQTEDirection.Up: return "↑";
                case EQTEDirection.Down: return "↓";
                case EQTEDirection.Left: return "←";
                case EQTEDirection.Right: return "→";
            }

            return "?";
        }
    }
}