using UnityEngine;

namespace LAMENT
{
    /// <summary> 최대 체력 </summary>
    [CreateAssetMenu(fileName = "MaxHP", menuName = "ScriptableObjects/GutEffects/MaxHp")]
    public sealed class GEMaxHP : GutEffectData
    {
        [SerializeField] private int bonus = 0;

        public override void Apply(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.AddMaxHpBonus(this, bonus);
        }

        public override void Remove(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.RemoveMaxHpBonus(this);
        }
    }
}