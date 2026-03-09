using UnityEngine;

namespace LAMENT
{
    public abstract class GutEffectData : ScriptableObject
    {
        public abstract void Apply(Player player);
        public abstract void Remove(Player player);
    }
}