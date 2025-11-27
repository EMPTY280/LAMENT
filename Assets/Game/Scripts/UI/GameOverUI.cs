using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LAMENT
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            GameManager.Eventbus.Subscribe<GEOnPlayerGameOver>(OnGameOver);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnPlayerGameOver>(OnGameOver);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void OnGameOver(GEOnPlayerGameOver e)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f; // 완전 정지 (재시작 전까지)
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f; // 재시작 전에 반드시 복구
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
    
}

