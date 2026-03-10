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
            Debug.Log("무게 보너스 미구현: API만 임시 구현됨");
        }

        public override void Remove(Player player)
        {
            Debug.Log("무게 보너스 미구현: API만 임시 구현됨");
        }
    }
}