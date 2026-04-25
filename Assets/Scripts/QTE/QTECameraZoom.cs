using UnityEngine;

namespace LAMENT
{
    [RequireComponent(typeof(Camera))]
    public class QTECameraZoom : MonoBehaviour
    {
        [Header("추적 대상")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset;

        [Header("줌")]
        [SerializeField] private float defaultSize = 5.625f;
        [SerializeField] private float qteZoomSize = 2.0f;
        [SerializeField] private float zoomInSpeed = 18.0f;
        [SerializeField] private float zoomOutSpeed = 24.0f;

        [Header("상태")]
        [SerializeField] private bool debugLog = false;

        private Camera cam;
        private bool isActive = false;
        private bool isZoomOut = false;
        private float targetSize;

        private void Awake()
        {
            cam = GetComponent<Camera>();

            cam.orthographic = true;
            cam.enabled = false;

            cam.clearFlags = CameraClearFlags.Depth;
            cam.depth = 10;

            cam.orthographicSize = defaultSize;
            targetSize = defaultSize;
        }

        private void OnEnable()
        {
            GameManager.Eventbus.Subscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Subscribe<GEOnQTEFinished>(OnQTEFinished);
        }

        private void OnDisable()
        {
            GameManager.Eventbus.Unsubscribe<GEOnQTEStarted>(OnQTEStarted);
            GameManager.Eventbus.Unsubscribe<GEOnQTEFinished>(OnQTEFinished);
        }

        private void LateUpdate()
        {
            if (!isActive && !isZoomOut)
                return;

            FollowTarget();
            UpdateZoom();
        }

        private void FollowTarget()
        {
            if (target == null)
                return;

            Vector3 pos = target.position;
            pos.x += offset.x;
            pos.y += offset.y;
            pos.z = transform.position.z;

            transform.position = pos;
        }

        private void UpdateZoom()
        {
            float speed = isZoomOut ? zoomOutSpeed : zoomInSpeed;

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetSize,
                speed * Time.unscaledDeltaTime
            );

            if (isZoomOut && Mathf.Abs(cam.orthographicSize - defaultSize) <= 0.01f)
            {
                cam.orthographicSize = defaultSize;
                cam.enabled = false;
                isActive = false;
                isZoomOut = false;

                if (debugLog)
                    Debug.Log("[QTE][CAM] Zoom Out Finished");
            }
        }

        private void OnQTEStarted(GEOnQTEStarted e)
        {
            isActive = true;
            isZoomOut = false;

            cam.enabled = true;
            cam.orthographicSize = defaultSize;
            targetSize = qteZoomSize;

            FollowTarget();

            if (debugLog)
                Debug.Log("[QTE][CAM] Zoom In");
        }

        private void OnQTEFinished(GEOnQTEFinished e)
        {
            if (!isActive)
                return;

            isZoomOut = true;
            targetSize = defaultSize;

            if (debugLog)
                Debug.Log("[QTE][CAM] Zoom Out");
        }
    }
}