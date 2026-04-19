using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
     [RequireComponent(typeof(Collider2D))]
    public sealed class MoneyPickup : MonoBehaviour
    {
       [SerializeField] private int amount = 1;
       [SerializeField] private float destroyDelay = 0f;

       public void SetUp(int amount)
        {
            this.amount = amount;
        }

        private void Reset()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if(!TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.gravityScale =0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.CompareTag("Player"))
                return;
            
            if(amount <= 0)
                return;
            
            GameManager.Money.Add(amount);
            
            if(destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }


    }

}
