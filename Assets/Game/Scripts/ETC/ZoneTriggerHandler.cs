using System;
using UnityEngine;

namespace LAMENT
{
    /// <summary> 트리거 이벤트 핸들러 </summary>
    public class ZoneTriggerHandler : MonoBehaviour
    {
        private Action<ZoneTriggerHandler, Collider2D> cbOnEnter;
        private Action<ZoneTriggerHandler, Collider2D> cbOnExit;

        public Action<ZoneTriggerHandler, Collider2D> CB_OnEnter { set { cbOnEnter = value; } }
        public Action<ZoneTriggerHandler, Collider2D> CB_OnExit { set { cbOnExit = value; } }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            cbOnEnter?.Invoke(this, collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            cbOnExit?.Invoke(this, collision);
        }
    }
}