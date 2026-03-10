using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "New Gut List", menuName = "ScriptableObjects/Gut List")]
    public class GutList : ScriptableObject
    {
        [SerializeField] private GutData[] list;
        public GutData[] List => list;
    }
}