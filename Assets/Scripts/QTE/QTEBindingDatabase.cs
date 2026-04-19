using System;
using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "QTEBindingDatabase", menuName = "ScriptableObjects/QTE/QTE Binding Database")]
    public sealed class QTEBindingDatabase : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField] private EEquipSlotType slotType;
            [SerializeField] private EquipmentData equipment;
            [SerializeField] private Skill skill;
            [SerializeField] private bool isBurst;
            [SerializeField] private QTEData qteData;

            public EEquipSlotType SlotType => slotType;
            public EquipmentData Equipment => equipment;
            public Skill Skill => skill;
            public bool IsBurst => isBurst;
            public QTEData QteData => qteData;
        }

        [SerializeField] private Entry[] entries;

        public bool TryGet(EEquipSlotType slotType, EquipmentData equipment, Skill skill, bool isBurst, out QTEData qteData)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    Entry entry = entries[i];

                    if (entry.SlotType != slotType)
                        continue;

                    if (entry.Equipment != equipment)
                        continue;

                    if (entry.Skill != skill)
                        continue;

                    if (entry.IsBurst != isBurst)
                        continue;

                    qteData = entry.QteData;
                    return qteData != null;
                }
            }

            qteData = null;
            return false;
        }
    }
}