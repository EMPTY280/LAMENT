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
                currMarker.TryEnter();
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CANCEL)))
                OnCoffinButtonClicked();

#if true
            void UpdateMarker(StageMarker m)
            {
                // 연결된 마커들을 모두 업데이트
                foreach (StageMarker.NeighborMarker sm in m.NextMarkers)
                    UpdateMarker(sm.marker);

                // 그 다음 자신 업데이트
                m.UpdateState();
            }

            // TODO: 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameManager.GameUnlock.Unlock("Stage1");
                GameManager.GameUnlock.Unlock("Stage2");
                UpdateMarker(currMarker);
            }
#endif
        }
    }
}