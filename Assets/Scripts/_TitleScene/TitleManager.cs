using UnityEngine;


namespace LAMENT
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private CursorRect cursor;
        [SerializeField] private Transform[] buttons;

        private void Awake()
        {
            cursor.SetParent(buttons[0]);

            CommonUIContorl cuc = GetComponent<CommonUIContorl>();
            cuc.VerticalMove = 1;
            cuc.HorizontalMove = 0;
            cuc.CB_OnConfirmed = (pos) =>
            {
                switch (pos)
                {
                    case 0:
                        OnStartButtonClicked();
                        break;
                    case 1:
                        OnQuitButtonClicked();
                        break;
                }
            };

            cuc.CB_OnPositionMoved = (pos) =>
            {
                switch (pos)
                {
                    case 0:
                        cursor.SetParent(buttons[0]);
                        break;
                    case 1:
                        cursor.SetParent(buttons[1]);
                        break;
                }
            };
        }

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