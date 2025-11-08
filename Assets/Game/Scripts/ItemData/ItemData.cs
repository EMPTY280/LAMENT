using UnityEngine;

namespace LAMENT
{
    public class ItemData : ScriptableObject
    {
        [Header("Item")]
        [SerializeField]
        private string id;

        [SerializeField]
        private string itemName;

        [SerializeField]
        private Sprite icon;

        [SerializeField]
        [Tooltip("Max count of stack")]
        private int stackCountMax = 1;

        [SerializeField]
        private float weight = 1;


        public string ID => id;
        public string ItemName => itemName;
        public Sprite Icon => icon;
        public int StackCount => stackCountMax;
        public float Weight => weight;
    }
}