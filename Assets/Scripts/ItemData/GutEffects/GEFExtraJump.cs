using UnityEngine;

namespace LAMENT
{
    /// <summary> 추가 점프 </summary>
    [CreateAssetMenu(fileName = "ExtraJump", menuName = "ScriptableObjects/GutEffects/ExtraJump")]
    public sealed class GEFExtraJump : GutEffectData
    {
        [SerializeField] private int value = 1;

        public override void Apply(Player player)
        {
            (player.MoveComponent as PlayerMoveComponent).AddExtraJump(value);
        }

        public override void Remove(Player player)
        {
            (player.MoveComponent as PlayerMoveComponent).AddExtraJump(-value);
        }
    }
}