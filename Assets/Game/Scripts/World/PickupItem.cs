using UnityEngine;

namespace LAMENT
{
    /// <summary>
    /// 플레이어가 트리거로 지나가면 인벤토리에 AddItem 시도.
    /// 정책: 한 개라도 들어가면 자기 자신 제거. 하나도 못 들어가면 바닥에 유지.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class PickupItem : MonoBehaviour
    {
        [Header("Pickup Data")]
        [SerializeField] private ItemData data;     // 지금은 EquipmentItem만 연결해 테스트
        [SerializeField] private int amount = 1;
        [SerializeField] private float destroyDelay = 0f;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // 트리거 검출을 보장하기 위해 RigidBody(kine) 권장
            if (!TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.gravityScale = 0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (data == null || amount <= 0)
                return;

            // 플레이어 오브젝트에 인벤토리 서비스가 붙어 있다고 가정(조립 시 주입)
            if (!other.TryGetComponent<InventoryService>(out var inv))
                return;

            var added = inv.AddItem(data, amount);
            if (added > 0)
            {
                if (destroyDelay > 0f) Destroy(gameObject, destroyDelay);
                else Destroy(gameObject);
            }
        }
    }
}