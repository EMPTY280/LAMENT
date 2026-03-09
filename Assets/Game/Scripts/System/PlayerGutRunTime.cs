using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    [System.Serializable]
    public sealed class PlayerGutRuntime
    {
        private readonly Dictionary<object, int> weightCapBonus = new();
        private readonly Dictionary<object, float> noConsumeChance = new();
        private readonly Dictionary<object, int> maxHpBonus = new();
        private readonly Dictionary<object, float> stomachGainMult = new();

        public int WeightCapacityBonus { get; private set; }
        public float NoConsumeChance01 { get; private set; }
        public int MaxHpBonus { get; private set; }
        public float StomachGainMult { get; private set; } = 1f;

        public void AddWeightCapacityBonus(object source, int bonus)
        {
            if (source == null) return;
            weightCapBonus[source] = bonus;
        }

        public void RemoveWeightCapacityBonus(object source)
        {
            if (source == null) return;
            weightCapBonus.Remove(source);
        }

        public void AddNoConsumeChance(object source, float chance01)
        {
            if (source == null) return;
            noConsumeChance[source] = Mathf.Clamp01(chance01);
        }

        public void RemoveNoConsumeChance(object source)
        {
            if (source == null) return;
            noConsumeChance.Remove(source);
        }

        public void AddMaxHpBonus(object source, int bonus)
        {
            if (source == null) return;
            maxHpBonus[source] = bonus;
        }

        public void RemoveMaxHpBonus(object source)
        {
            if (source == null) return;
            maxHpBonus.Remove(source);
        }

        public void AddStomachGainMult(object source, float mult)
        {
            if (source == null) return;
            stomachGainMult[source] = Mathf.Max(0f, mult);
        }

        public void RemoveStomachGainMult(object source)
        {
            if (source == null) return;
            stomachGainMult.Remove(source);
        }

        public void RecalculateAndPublish()
        {
            int weight = 0;
            foreach (var kv in weightCapBonus)
                weight += kv.Value;

            int hp = 0;
            foreach (var kv in maxHpBonus)
                hp += kv.Value;

            float stomachMult = 1f;
            foreach (var kv in stomachGainMult)
                stomachMult *= kv.Value;
            stomachMult = Mathf.Max(0f, stomachMult);

            float product = 1f;
            foreach (var kv in noConsumeChance)
                product *= (1f - Mathf.Clamp01(kv.Value));

            WeightCapacityBonus = weight;
            MaxHpBonus = hp;
            StomachGainMult = stomachMult;
            NoConsumeChance01 = 1f - Mathf.Clamp01(product);

            GameManager.Eventbus.Publish(new GEOnGutsRuntimeChanged(
                WeightCapacityBonus,
                NoConsumeChance01,
                MaxHpBonus,
                StomachGainMult));
        }

        public bool RollPreventConsume()
        {
            float p = Mathf.Clamp01(NoConsumeChance01);
            if (p <= 0f)
                return false;

            return Random.value < p;
        }
    }
}