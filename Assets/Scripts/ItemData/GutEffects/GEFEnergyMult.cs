using UnityEngine;

namespace LAMENT
{
    /// <summary> 위 게이지 획득 배율 </summary>
    [CreateAssetMenu(fileName = "EnergyMult", menuName = "ScriptableObjects/GutEffects/EnergyMult")]
    public sealed class GEFEnergyMult : GutEffectData
    {
        [SerializeField] private float value = 1f;

        public override void Apply(Player player)
        {
            player.AddEnergyMultAttribute(value);
        }

        public override void Remove(Player player)
        {
            player.AddEnergyMultAttribute(-value);
        }
    }
    
}