using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "New Scene Sound Profile List", menuName = "ScriptableObjects/Sound/Scene Sound Profile List")]
    public class SceneSoundProfileList : ScriptableObject
    {
        [SerializeField] private SceneSoundProfile[] profiles;

        public bool TryGet(string sceneName, out SceneSoundProfile profile)
        {
            profile = null;

            if (profiles == null)
                return false;

            for (int i = 0; i < profiles.Length; i++)
            {
                if (!profiles[i])
                    continue;

                if (profiles[i].SceneName == sceneName)
                {
                    profile = profiles[i];
                    return true;
                }
            }

            return false;
        }
    }
}