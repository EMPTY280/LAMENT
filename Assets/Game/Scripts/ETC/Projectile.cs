using System;
using UnityEngine;

namespace LAMENT
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;

        private Action<Projectile, Collider2D> cbOnHit;


        private void Awake()
        {
            SetActive(false);
        }

        public void Fire(Vector2 pos, Vector2 vector, Action<Projectile, Collider2D> cbOnHit)
        {
            gameObject.SetActive(true);

            transform.position = pos;
            rb.velocity = vector;

            this.cbOnHit = cbOnHit;
        }

        public void SetActive(bool b)
        {
            gameObject.SetActive(b);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (cbOnHit != null)
                cbOnHit(this, collision);
        }
    }
}