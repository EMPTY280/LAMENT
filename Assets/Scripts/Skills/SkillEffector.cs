using System;
using UnityEngine;

namespace LAMENT
{
    /// <summary> 무기의 히트 판정, 스프라이트 표시 등을 담당하는 클래스 </summary>
    public class SkillEffector : MonoBehaviour
    {
        private GameObject[] childs;

        private Entity owner;
        private Action<IHittable> cbOnHitTarget;

        public Action<IHittable> CB_OnHitTarget { set { cbOnHitTarget = value; } } 


        private void Awake()
        {
            childs = new GameObject[transform.childCount];
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i] = transform.GetChild(i).gameObject;
                childs[i].SetActive(false);
            }
        }

        public void SetEnabled(int idx, bool isEnabled)
        {
            if (idx < 0 || childs.Length <= idx)
            {
                GameManager.Logger.LogError("히트박스 인덱스가 범위를 벗어났습니다.");
                return;
            }

            childs[idx].SetActive(isEnabled);
        }

        public void SetOwner(Entity entity)
        {
            owner = entity;
        }

        public void FlipY(bool isFlipped)
        {
            Vector3 v = transform.localScale;
            v.y = Mathf.Abs(v.y) * (isFlipped ? -1 : 1);
            transform.localScale = v;
        }

        public void OnTriggered(Collider2D collision)
        {
            // 같은 진영은 타격하지 않음
            if (owner.CompareTag(collision.tag))
                return;

            if (collision.TryGetComponent(out IHittable target))
            {
                DamageResponse rsp = new();
                rsp.src = owner;

                target.OnHit(rsp);
                if (cbOnHitTarget != null)
                    cbOnHitTarget(target);
            }
        }
    }
}