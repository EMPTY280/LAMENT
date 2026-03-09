using UnityEngine;

namespace LAMENT
{
    /// <summary> 추가 점프 </summary>
    [CreateAssetMenu(fileName = "ExtraJump", menuName = "ScriptableObjects/GutEffects/ExtraJump")]
    public sealed class GEExtraJump : GutEffectData
    {
        [SerializeField] private int extraJumpCount = 1;

        public override void Apply(Player player)
        {
            if (!player)
                return;

            if (player.MoveComponent is PlayerMoveComponent pm)
            {
                pm.AddExtraJump(this, extraJumpCount);
                Debug.Log($"[GUT][EFFECT] Apply ExtraJump +{extraJumpCount}");
            }
        }

        public override void Remove(Player player)
        {
            if (!player)
                return;

            if (player.MoveComponent is PlayerMoveComponent pm)
            {
                pm.RemoveExtraJump(this);
                Debug.Log($"[GUT][EFFECT] Remove ExtraJump");
            }
        }
    }
}