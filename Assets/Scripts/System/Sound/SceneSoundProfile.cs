using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "New Scene Sound Profile", menuName = "ScriptableObjects/Sound/Scene Sound Profile")]
    public class SceneSoundProfile : ScriptableObject
    {
        [Header("씬 이름")]
        [SerializeField] private string sceneName;

        [Header("BGM")]
        [SerializeField] private string bgmId;
        [SerializeField] private bool keepCurrentBgm = false;
        [SerializeField] private bool stopBgm = false;
        [SerializeField] private float fadeDuration = 0.5f;

        public string SceneName => sceneName;
        public string BgmId => bgmId;
        public bool KeepCurrentBgm => keepCurrentBgm;
        public bool StopBgm => stopBgm;
        public float FadeDuration => fadeDuration;
    }
}