using System.Collections;
using UnityEngine;



public class PlatformDropDown : MonoBehaviour
{
    [SerializeField] private float passThroughTime = 0.3f;  // 통과 지속 시간

    private Collider2D m_collider;

    void Awake()
    {
        m_collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // 아래 방향키만 입력
        bool dropInput = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

        if (dropInput)
        {
            StartCoroutine(DropThrough());
        }
    }

    private IEnumerator DropThrough()
    {
        // 현재 닿아있는 발판 콜라이더 찾기
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int count = m_collider.GetContacts(contacts);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = contacts[i].collider;

            // Platform Effector 2D 가 붙어있는 발판만 처리
            if (other.TryGetComponent<PlatformEffector2D>(out _))
            {
                Physics2D.IgnoreCollision(m_collider, other, true);
                yield return new WaitForSeconds(passThroughTime);
                Physics2D.IgnoreCollision(m_collider, other, false);
            }
        }
    }
}