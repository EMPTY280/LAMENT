using UnityEngine;

namespace LAMENT
{
    public enum EGutType
    {
        HEART = 0,
        LUNGS = 1,
        LIVER = 2,
        STOMACH = 3,
        INTESTINE = 4,


        _LENGTH = 5, // 열거형 길이 파악용. 항상 맨 마지막에 위치
    }

    [CreateAssetMenu(fileName = "New Gut", menuName = "ScriptableObjects/Gut")]
    public class GutData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private string name;
        [SerializeField] private string desc;
        [SerializeField] private string unlockMethod;

        public string ID => id;
        public Sprite Icon => icon;
        public string Name => name;
        public string Desc => desc;
        public string UnlockMethod => unlockMethod;
    }
}