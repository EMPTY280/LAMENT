using UnityEngine;

namespace LAMENT
{
    public class MonsterIdle : Entity
    {
        [Header("Item")]
        public GameObject dropitem;
        public Transform dropPoint;


        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnDamageHandled(DamageResponse rsp)
        {
            Instantiate(dropitem, dropPoint.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}