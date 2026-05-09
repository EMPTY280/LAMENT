using UnityEngine;

namespace LAMENT
{
    /// <summary> 플레이어가 도착 지점에 닿으면 스테이지를 클리어 처리하고 다음 씬으로 이동한다. </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StageClearTrigger : MonoBehaviour
    {
        [Header("클리어")]
        [SerializeField] private string clearID = "";

        [Header("이동")]
        [SerializeField] private string nextSceneName = "Worldmap";
        [SerializeField] private float transitionDuration = 1f;

        private bool isTriggered = false;

        private void Reset()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered)
                return;

            if (!other.CompareTag("Player"))
                return;

            isTriggered = true;

            GameManager.GameUnlock.Unlock(clearID);
            GameManager.Instance.TryChangeScene(nextSceneName, transitionDuration);
        }
    }
}
