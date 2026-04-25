using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class SoundLibrary
    {
         private const string RESOURCE_PATH = "Sounds";

        private Dictionary<string, SoundData> map = new Dictionary<string, SoundData>();
        private bool isInit = false;

        public void Init()
        {
            if (isInit)
                return;

            isInit = true;
            map.Clear();

            SoundData[] allSounds = Resources.LoadAll<SoundData>(RESOURCE_PATH);

            for (int i = 0; i < allSounds.Length; i++)
            {
                SoundData data = allSounds[i];

                if (!data)
                    continue;

                if (string.IsNullOrEmpty(data.ID))
                {
                    Debug.Log("[SOUND] ID가 비어있는 SoundData가 있습니다.");
                    continue;
                }

                if (!data.Clip)
                {
                    Debug.Log($"[SOUND] Clip이 없는 SoundData: {data.ID}");
                    continue;
                }

                if (map.ContainsKey(data.ID))
                {
                    Debug.Log($"[SOUND] 중복 Sound ID: {data.ID}");
                    continue;
                }

                map.Add(data.ID, data);
            }

            Debug.Log($"[SOUND] Loaded Sound Count: {map.Count}");
        }

        public bool TryGet(string id, out SoundData data)
        {
            if (!isInit)
                Init();

            data = null;

            if (string.IsNullOrEmpty(id))
                return false;

            return map.TryGetValue(id, out data);
        }
    }

}
