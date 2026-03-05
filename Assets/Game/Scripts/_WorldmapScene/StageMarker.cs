using System;
using LAMENT;
using UnityEngine;
using UnityEngine.UI;

public class StageMarker : MonoBehaviour
{
    [Serializable]
    private struct NeighborMarker
    {
        public StageMarker marker;
        public GameManager.KeyMap.EKey dir;
        [HideInInspector] public Image line;
    }

    [SerializeField] private Image image;
    [SerializeField] private string targetSceneName;

    [Header("인접 마커")]
    [SerializeField] private NeighborMarker prevMarker;
    [SerializeField] private NeighborMarker[] nextMarkers;
    [SerializeField] private GameObject linePrefab;

    [Header("언락 및 클리어")]
    [SerializeField] private bool isUnlocked = false;
    [SerializeField] private string clearID = "";
    public bool IsUnlocked => isUnlocked;


#if UNITY_EDITOR

    /// <summary> 에디터에서 화살표 그리기 </summary>
    private void DrawNeighborLine(Color c)
    {
        Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;

            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        Gizmos.color = c;
        foreach (NeighborMarker nm in nextMarkers)
        {
            if (!nm.marker)
                continue;

            Gizmos.DrawLine(transform.position, nm.marker.transform.position);

            Vector2 dir = (nm.marker.transform.position - transform.position).normalized;

            Vector2 right = Rotate(dir, 180 + 30);
            Vector2 left = Rotate(dir, 180 - 30);

            Gizmos.DrawLine(nm.marker.transform.position, nm.marker.transform.position + (Vector3)right * 0.3f);
            Gizmos.DrawLine(nm.marker.transform.position, nm.marker.transform.position + (Vector3)left * 0.3f);
        }
    }

    private void OnDrawGizmos()
    {
        DrawNeighborLine(Color.green);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawNeighborLine(Color.yellow);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        if (prevMarker.marker != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(prevMarker.marker.transform.position, 0.3f);
        }
    }

#endif

    private void Awake()
    {
        // 노드들 서로 연결
        int iter = 0;
        foreach (NeighborMarker nm in nextMarkers)
        {
            Vector2 dir = nm.marker.transform.position - transform.position;
            Vector2 centerPos = (nm.marker.transform.position + transform.position) * 0.5f;

            RectTransform t = Instantiate(linePrefab, transform).GetComponent<RectTransform>();
            t.position = centerPos;
            t.sizeDelta = new Vector2(dir.magnitude * 64 - 32, 32);
            t.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            nextMarkers[iter].line = t.GetComponent<Image>();

            iter++;
        }

        SetAsLocked();
        UpdateState();
    }

    public void UpdateState()
    {
        if (isUnlocked)
            SetAsUnlocked();
        if (clearID != "" && GameManager.GameUnlock.IsUnlocked(clearID))
            SetAsCleared();
    }

    public StageMarker GetNeighbor(GameManager.KeyMap.EKey dir)
    {
        foreach (NeighborMarker nm in nextMarkers)
        {
            if (nm.dir == dir)
                return nm.marker;
        }

        if (prevMarker.marker != null && prevMarker.dir == dir)
            return prevMarker.marker;

        return null;
    }

    private void SetAsLocked()
    {
        image.color = Color.gray;

        foreach (NeighborMarker nm in nextMarkers)
            nm.line.color = Color.grey;
    }

    public void SetAsUnlocked()
    {
        image.color = Color.white;
        isUnlocked = true;
    }

    public void SetAsCleared()
    {
        foreach (NeighborMarker nm in nextMarkers)
        {
            nm.line.color = Color.white;
            nm.marker.SetAsUnlocked();
        }
    }

    public void TryEnter()
    {
        if (isUnlocked)
            GameManager.Instance.TryChangeScene(targetSceneName);
    }
}
