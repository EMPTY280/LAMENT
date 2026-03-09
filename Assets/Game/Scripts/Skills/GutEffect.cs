using System;
using UnityEngine;

namespace LAMENT
{
      public abstract class GutEffect : ScriptableObject
    {
        public abstract void Apply(Player player);
        public abstract void Remove(Player player);
    }

    // =========================
    // 1) 최대 무게 증가
    // =========================
    [CreateAssetMenu(fileName = "GutEffect_WeightCapacity", menuName = "ScriptableObjects/GutEffects/WeightCapacity")]
    public sealed class GutEffect_WeightCapacity : GutEffect
    {
        [SerializeField] private int bonus = 0;

        public override void Apply(Player player)
        {
            if (!player) return;
            player.GutRuntime.AddWeightCapacityBonus(this, bonus);
        }

        public override void Remove(Player player)
        {
            if (!player) return;
            player.GutRuntime.RemoveWeightCapacityBonus(this);
        }
    }

    // =========================
    // 2) Burst 장비 소모 방지 확률
    // =========================
    [CreateAssetMenu(fileName = "GutEffect_NoConsumeChance", menuName = "ScriptableObjects/GutEffects/NoConsumeChance")]
    public sealed class GutEffect_NoConsumeChance : GutEffect
    {
        [Range(0f, 1f)]
        [SerializeField] private float chance01 = 0f;

        public override void Apply(Player player)
        {
            if (!player) return;
            player.GutRuntime.AddNoConsumeChance(this, chance01);
        }

        public override void Remove(Player player)
        {
            if (!player) return;
            player.GutRuntime.RemoveNoConsumeChance(this);
        }
    }

    // =========================
    // 3) 최대 체력 증가
    // =========================
    [CreateAssetMenu(fileName = "GutEffect_MaxHpBonus", menuName = "ScriptableObjects/GutEffects/MaxHpBonus")]
    public sealed class GutEffect_MaxHpBonus : GutEffect
    {
        [SerializeField] private int bonus = 0;

        public override void Apply(Player player)
        {
            if (!player) return;
            player.GutRuntime.AddMaxHpBonus(this, bonus);
        }

        public override void Remove(Player player)
        {
            if (!player) return;
            player.GutRuntime.RemoveMaxHpBonus(this);
        }
    }

    // =========================
    // 4) 위 게이지 획득 배율
    // =========================
    [CreateAssetMenu(fileName = "GutEffect_StomachGainMult", menuName = "ScriptableObjects/GutEffects/StomachGainMult")]
    public sealed class GutEffect_StomachGainMult : GutEffect
    {
        [SerializeField] private float mult = 1f;

        public override void Apply(Player player)
        {
            if (!player) return;
            player.GutRuntime.AddStomachGainMult(this, mult);
        }

        public override void Remove(Player player)
        {
            if (!player) return;
            player.GutRuntime.RemoveStomachGainMult(this);
        }
    }

    // =========================
    // 5) 더블 점프 / 추가 점프
    // =========================
    [CreateAssetMenu(fileName = "GutEffect_ExtraJump", menuName = "ScriptableObjects/GutEffects/ExtraJump")]
    public sealed class GutEffect_ExtraJump : GutEffect
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