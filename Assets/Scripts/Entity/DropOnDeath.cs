using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class DropOnDeath : MonoBehaviour
    {
        [SerializeField] 
        private struct DropEntry
        {
            public ItemData item;
            [Min(1)]public int minAmount;
            [Min(1)]public int maxAmount;
        }

        [Header("Target Entity")]
        [SerializeField] private Entity target;

         [Header("Pickup Prefab")]
        [SerializeField] private PickupItem pickupPrefab; // 드랍에 사용할 PickupItem 프리팹

        [Header("Drop Table (랜덤 후보들)")]
        [SerializeField] private DropEntry[] dropTable;   // 여기 중 하나 랜덤 선택

        [Header("Drop Chance")]
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 1f;   // 1 = 100%

        private void Awake()
        {
            if(target == null)
                target = GetComponent<Entity>();
            
            GameManager.Eventbus.Subscribe<GEOnEntityDied>(OnEntityDied);
        }

        private void Oestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEntityDied>(OnEntityDied);           
        }

        private void OnEntityDied(GEOnEntityDied e)
        {
            if(e.Dead != target)
                return;

            TryDropRandom();
        }

        private void TryDropRandom()
        {
            if (pickupPrefab == null)
                return;

            if (dropTable == null || dropTable.Length == 0)
                return;

            // 드랍 확률 체크
            if (UnityEngine.Random.value > dropChance)
                return;

            // 드랍 후보 중 하나 랜덤 선택
            var entry = dropTable[UnityEngine.Random.Range(0, dropTable.Length)];
            if (entry.item == null)
                return;

            // 최소~최대 개수 랜덤
            int amount = UnityEngine.Random.Range(entry.minAmount, entry.maxAmount + 1);
            if (amount <= 0)
                return;

            // PickupItem 생성 + 데이터 세팅
            var pickup = Instantiate(pickupPrefab, transform.position, Quaternion.identity);
            pickup.Setup(entry.item, amount);
        }

    }

}
