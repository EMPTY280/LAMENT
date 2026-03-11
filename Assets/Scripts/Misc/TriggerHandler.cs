using System;
using UnityEngine;

namespace LAMENT
{
    /// <summary> 트리거 이벤트 핸들러 </summary>
    public class TriggerHandler : MonoBehaviour, IHittable
    {
        // ===== 소유자 =====
        [Header("소유자 - 할당 시 받은 공격을 소유자에게 전달함"), SerializeField]
        private Entity owner;

        // ===== 콜백 =====
        private Action<TriggerHandler, Collider2D> cbOnEnter;
        private Action<TriggerHandler, Collider2D> cbOnExit;

        public Action<TriggerHandler, Collider2D> CB_OnEnter { set { cbOnEnter = value; } }
        public Action<TriggerHandler, Collider2D> CB_OnExit { set { cbOnExit = value; } }


        public bool OnHitTaken(DamageResponse rsp)
        {
            return owner.OnHitTaken(rsp);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (cbOnEnter != null)
                cbOnEnter(this, collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (cbOnExit != null)
                cbOnExit(this, collision);
        }

        public bool TryGetOwner(out Entity entity)
        {
            entity = owner;
            return owner != null;
        }
    }
}