using UnityEngine;

namespace LAMENT
{
    /// <summary> 파괴 방지 확률 </summary>
    [CreateAssetMenu(fileName = "ConsumeChance", menuName = "ScriptableObjects/GutEffects/ConsumeChance")]
    public sealed class GEConsumeChance : GutEffectData
    {
        [Range(0f, 1f)]
        [SerializeField] private float chance01 = 0f;

        public override void Apply(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.AddNoConsumeChance(this, chance01);
        }

        public override void Remove(Player player)
        {
            if (!player)
                return;
            player.GutRuntime.RemoveNoConsumeChance(this);
        }
    }
}