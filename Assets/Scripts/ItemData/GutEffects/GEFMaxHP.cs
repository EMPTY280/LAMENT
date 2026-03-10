using UnityEngine;

namespace LAMENT
{
    /// <summary> 최대 체력 </summary>
    [CreateAssetMenu(fileName = "MaxHP", menuName = "ScriptableObjects/GutEffects/MaxHp")]
    public sealed class GEFMaxHP : GutEffectData
    {
        [SerializeField] private int value = 0;

        public override void Apply(Player player)
        {
            player.AddMaxHPAttribute(value);
        }

        public override void Remove(Player player)
        {
            player.AddMaxHPAttribute(value);
        }
    }
}