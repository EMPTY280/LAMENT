using UnityEngine;

namespace LAMENT
{
    /// <summary> 위 게이지 획득 배율 </summary>
    [CreateAssetMenu(fileName = "EnergyMult", menuName = "ScriptableObjects/GutEffects/EnergyMult")]
    public sealed class GEEnergyMult : GutEffectData
    {
        [SerializeField] private float mult = 1f;

        public override void Apply(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.AddStomachGainMult(this, mult);
        }

        public override void Remove(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.RemoveStomachGainMult(this);
        }
    }
    
}