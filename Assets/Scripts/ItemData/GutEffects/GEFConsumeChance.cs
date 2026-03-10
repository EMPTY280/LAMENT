using UnityEngine;

namespace LAMENT
{
    /// <summary> 파괴 확률 </summary>
    [CreateAssetMenu(fileName = "ConsumeChance", menuName = "ScriptableObjects/GutEffects/ConsumeChance")]
    public sealed class GEFConsumeChance : GutEffectData
    {
        [Range(-1f, 1f)]
        [SerializeField] private float value = 0f;

        public override void Apply(Player player)
        {
            player.AddConsumeChanceAttribute(value);
        }

        public override void Remove(Player player)
        {
            player.AddConsumeChanceAttribute(-value);
        }
    }
}