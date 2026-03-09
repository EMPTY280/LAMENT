using LAMENT;
using UnityEngine;

public class SESegment : MonoBehaviour
{
    private SkillEffector owner;


    private void Awake()
    {
        owner = transform.parent.GetComponent<SkillEffector>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        owner.OnTriggered(collision);
    }
}
