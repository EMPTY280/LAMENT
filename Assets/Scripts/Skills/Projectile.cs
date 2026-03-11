using System;
using UnityEngine;

namespace LAMENT
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;

        private Entity owner;
        public Action<IHittable, Collider2D> CB_OnHitTarget { set; private get; }
        public Action<Collider2D> CB_OnHitTerrain { set; private get; }


        private void Awake()
        {
            SetActive(false);
        }

        public void Fire(Entity owner, Vector2 pos, Vector2 vector)
        {
            this.owner = owner;

            gameObject.SetActive(true);

            transform.position = pos;
            rb.velocity = vector;
        }

        public void SetActive(bool b)
        {
            gameObject.SetActive(b);
            if (!b)
            {
                owner = null;
                CB_OnHitTarget = null;
                CB_OnHitTerrain = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            switch (col.gameObject.layer)
            {
                case 6:
                    // 같은 태그를 지닌 대상은 무시
                    if (col.CompareTag(owner?.tag))
                        return;

                    if (CB_OnHitTarget != null && col.TryGetComponent(out IHittable target))
                        CB_OnHitTarget(target, col);

                break;

                case 3: // Terrain
                    if (CB_OnHitTerrain != null)
                        CB_OnHitTerrain(col);
                break;
            }

            SetActive(false);
        }
    }
}