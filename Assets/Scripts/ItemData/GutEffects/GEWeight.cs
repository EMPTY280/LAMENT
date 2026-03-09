using UnityEngine;

namespace LAMENT
{
    /// <summary> 최대 무게 </summary>
    [CreateAssetMenu(fileName = "Weight", menuName = "ScriptableObjects/GutEffects/Weight")]
    public sealed class GEWeight : GutEffectData
    {
        [SerializeField] private int bonus = 0;

        public override void Apply(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.AddWeightCapacityBonus(this, bonus);
        }

        public override void Remove(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.RemoveWeightCapacityBonus(this);
        }
    }
}