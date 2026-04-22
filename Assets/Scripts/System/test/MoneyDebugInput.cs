using UnityEngine;

namespace LAMENT
{
    public class MoneyDebugInput : MonoBehaviour
    {
        [SerializeField] private int addAmount = 1000;
        [SerializeField] private int setAmount = 99999;

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F5))
            {
                GameManager.Money.Add(addAmount);
                Debug.Log($"[DEBUG][MONEY] add = {addAmount}, current = {GameManager.Money.Get()}");
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                GameManager.Money.Set(setAmount);
                Debug.Log($"[DEBUG][MONEY] set = {GameManager.Money.Get()}");
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                GameManager.Money.Clear();
                Debug.Log("[DEBUG][MONEY] clear");
            }
#endif
        }
    }
}