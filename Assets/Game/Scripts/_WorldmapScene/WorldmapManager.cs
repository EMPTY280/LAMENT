using UnityEngine;

namespace LAMENT
{
    public class WorldmapManager : MonoBehaviour
    {
        [SerializeField] private StageMarker currMarker;
        [SerializeField] private CursorRect cursor;


        private void Awake()
        {
            cursor.SetParent(currMarker.transform);
        }

        public void OnCoffinButtonClicked()
        {
            GameManager.Instance.TryChangeScene("Collections");
        }

        private void Update()
        {
            StageMarker sm = null;

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP)))
                sm = currMarker.GetNeighbor(GameManager.KeyMap.EKey.UP);
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN)))
                sm = currMarker.GetNeighbor(GameManager.KeyMap.EKey.DOWN);
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT)))
                sm = currMarker.GetNeighbor(GameManager.KeyMap.EKey.LEFT);
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT)))
                sm = currMarker.GetNeighbor(GameManager.KeyMap.EKey.RIGHT);

            if (sm != null && sm.IsUnlocked)
            {
                currMarker = sm;
                cursor.SetParent(sm.transform);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CONFIRM)))
                currMarker.Enter();
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CANCEL)))
                OnCoffinButtonClicked();

#if UNITY_EDITOR
            // TODO: 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameManager.GameUnlock.Unlock("Stage1");
                currMarker.UpdateState();
            }
        }
#endif
    }
}