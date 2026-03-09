using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class testSceneMove : MonoBehaviour
{
     [Header("이동할 플레이 씬")]
        [SerializeField] private string playSceneName = "PlayScene";

        [Header("테스트 입력")]
        [SerializeField] private KeyCode moveKey = KeyCode.Return;

        private void Update()
        {
            if (Input.GetKeyDown(moveKey))
                MoveToPlayScene();
        }

        public void MoveToPlayScene()
        {
            if (string.IsNullOrEmpty(playSceneName))
            {
                Debug.LogError("[GoToPlayScene] 플레이 씬 이름이 비어 있습니다.");
                return;
            }

            SceneManager.LoadScene(playSceneName);
        }
}
