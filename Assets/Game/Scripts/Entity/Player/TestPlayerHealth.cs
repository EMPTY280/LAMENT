using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class TestPlayerHealth : MonoBehaviour
    {
        private PlayerHealth health;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (health == null) return;

            // 1 키: 데미지 1
            if (Input.GetKeyDown(KeyCode.L))
            {
                health.TakeHit(1);
                Debug.Log($"[TEST] Hit: HP = {health.CurrentHp}/{health.CurrentMaxHp}");
            }

            // 2 키: 적 공격 성공 (위 게이지 소량 증가)
            if (Input.GetKeyDown(KeyCode.K))
            {
                health.OnAttackLanded();
                Debug.Log($"[TEST] AttackLanded: Stomach = {health.StomachCurr}");
            }

            // 3 키: 팔/다리 섭취 (위 게이지 많이 증가)
            if (Input.GetKeyDown(KeyCode.J))
            {
                health.OnLimbConsumed();
                Debug.Log($"[TEST] LimbConsumed: Stomach = {health.StomachCurr}");
            }
        }
    }
    

}
