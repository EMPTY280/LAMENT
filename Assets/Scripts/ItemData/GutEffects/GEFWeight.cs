using UnityEngine;

namespace LAMENT
{
    /// <summary> 최대 무게 </summary>
    [CreateAssetMenu(fileName = "Weight", menuName = "ScriptableObjects/GutEffects/Weight")]
    public sealed class GEFWeight : GutEffectData
    {
        [SerializeField] private int bonus = 0;

        public override void Apply(Player player)
        {
            player.AddWeightCapacityAttribute(bonus);
        }

        public override void Remove(Player player)
        {
            player.AddWeightCapacityAttribute(-bonus);
        }
    }
}
