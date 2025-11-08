using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: 임시 구현
public class FollowCam : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Vector2 offset;

    [SerializeField] private float yMin = -2f;
    [SerializeField] private float lerpMult = 0.1f;

    private void Update()
    {
        if (target == null)
            return;

        Vector3 newPos = target.transform.position;
        newPos.x += offset.x;
        newPos.y += offset.y;
        newPos.y = Mathf.Max(newPos.y, yMin);
        newPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, newPos, lerpMult);
    }
}
