using UnityEngine;


public class FollowCam : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 0.1f;

    [Header("카메라 좌표")]
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 xLock;
    [SerializeField] private Vector2 yLock;


#if UNITY_EDITOR

    private void OnValidate()
    {
        FollowTarget(99999);
    }

#endif

    private void Update()
    {
        FollowTarget(followSpeed);
    }

    private void FollowTarget(float speed)
    {
        if (target == null)
            return;

        Vector3 newPos = target.position;
        newPos.x += offset.x;
        newPos.y += offset.y;

        newPos.x = Mathf.Clamp(newPos.x, xLock.x, xLock.y);
        newPos.y = Mathf.Clamp(newPos.y, yLock.x, yLock.y);
        newPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);
    }
}
