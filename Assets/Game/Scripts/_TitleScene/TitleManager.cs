using UnityEngine;


namespace LAMENT
{
    public class TitleManager : MonoBehaviour
    {
        public void OnStartButtonClicked()
        {
            GameManager.Instance.TryChangeScene("Worldmap");
        }

        public void OnQuitButtonClicked()
        {
            Application.Quit();
        }
    }
}