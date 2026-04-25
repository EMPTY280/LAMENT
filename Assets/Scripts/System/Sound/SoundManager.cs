using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LAMENT
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("씬별 BGM 설정")]
        [SerializeField] private SceneSoundProfileList sceneProfiles;

        [Header("Pool")]
        [SerializeField] private int sfxPoolSize = 16;
        [SerializeField] private int uiPoolSize = 6;
        [SerializeField] private int ambientPoolSize = 4;

        [Header("Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float uiVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float ambientVolume = 1f;

        private SoundLibrary library = new SoundLibrary();

        private AudioSource bgmSource;
        private List<AudioSource> sfxSources = new List<AudioSource>();
        private List<AudioSource> uiSources = new List<AudioSource>();
        private List<AudioSource> ambientSources = new List<AudioSource>();

        private Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();

        private string currentBgmId = "";
        private Coroutine bgmFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Init();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Init()
        {
            library.Init();

            bgmSource = CreateSource("BGM_Source");
            bgmSource.loop = true;

            CreatePool(sfxSources, sfxPoolSize, "SFX_Source");
            CreatePool(uiSources, uiPoolSize, "UI_Source");
            CreatePool(ambientSources, ambientPoolSize, "Ambient_Source");
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject obj = new GameObject(sourceName);
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            return source;
        }

        private void CreatePool(List<AudioSource> list, int count, string sourceName)
        {
            for (int i = 0; i < count; i++)
                list.Add(CreateSource($"{sourceName}_{i}"));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplySceneSound(scene.name);
        }

        public void ApplySceneSound(string sceneName)
        {
            if (!sceneProfiles)
                return;

            SceneSoundProfile profile;
            if (!sceneProfiles.TryGet(sceneName, out profile))
                return;

            if (profile.KeepCurrentBgm)
                return;

            if (profile.StopBgm)
            {
                StopBGM(profile.FadeDuration);
                return;
            }

            if (!string.IsNullOrEmpty(profile.BgmId))
                PlayBGM(profile.BgmId, profile.FadeDuration);
        }

        public void PlayBGM(string id, float fadeDuration = 0.5f)
        {
            if (currentBgmId == id && bgmSource.isPlaying)
                return;

            SoundData data;
            if (!TryGetSound(id, out data))
                return;

            currentBgmId = id;

            if (bgmFadeCoroutine != null)
                StopCoroutine(bgmFadeCoroutine);

            bgmFadeCoroutine = StartCoroutine(CoChangeBGM(data, fadeDuration));
        }

        private IEnumerator CoChangeBGM(SoundData data, float fadeDuration)
        {
            float fromVolume = bgmSource.volume;

            if (bgmSource.isPlaying && fadeDuration > 0f)
            {
                float time = 0f;

                while (time < fadeDuration)
                {
                    time += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(fromVolume, 0f, time / fadeDuration);
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = data.Clip;
            bgmSource.loop = data.Loop;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float targetVolume = data.Volume * bgmVolume * masterVolume;

            if (fadeDuration > 0f)
            {
                float time = 0f;

                while (time < fadeDuration)
                {
                    time += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(0f, targetVolume, time / fadeDuration);
                    yield return null;
                }
            }

            bgmSource.volume = targetVolume;
        }

        public void StopBGM(float fadeDuration = 0.5f)
        {
            currentBgmId = "";

            if (bgmFadeCoroutine != null)
                StopCoroutine(bgmFadeCoroutine);

            bgmFadeCoroutine = StartCoroutine(CoStopBGM(fadeDuration));
        }

        private IEnumerator CoStopBGM(float fadeDuration)
        {
            float fromVolume = bgmSource.volume;

            if (fadeDuration > 0f)
            {
                float time = 0f;

                while (time < fadeDuration)
                {
                    time += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(fromVolume, 0f, time / fadeDuration);
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = null;
            bgmSource.volume = 0f;
        }

        public void PlaySFX(string id)
        {
            Play(id, sfxSources);
        }

        public void PlayUI(string id)
        {
            Play(id, uiSources);
        }

        public void PlayAmbient(string id)
        {
            Play(id, ambientSources);
        }

        public void Play(string id)
        {
            SoundData data;
            if (!TryGetSound(id, out data))
                return;

            switch (data.Category)
            {
                case ESoundCategory.BGM:
                    PlayBGM(id);
                    break;

                case ESoundCategory.SFX:
                    PlaySFX(id);
                    break;

                case ESoundCategory.UI:
                    PlayUI(id);
                    break;

                case ESoundCategory.Ambient:
                    PlayAmbient(id);
                    break;
            }
        }

        private void Play(string id, List<AudioSource> pool)
        {
            SoundData data;
            if (!TryGetSound(id, out data))
                return;

            if (!CanPlay(data, pool))
                return;

            AudioSource source = GetPlayableSource(pool);

            if (!source)
                return;

            source.clip = data.Clip;
            source.loop = data.Loop;
            source.volume = GetFinalVolume(data);
            source.Play();

            lastPlayTimes[data.ID] = Time.unscaledTime;
        }

        private bool TryGetSound(string id, out SoundData data)
        {
            if (!library.TryGet(id, out data))
            {
                Debug.Log($"[SOUND] Sound ID를 찾을 수 없습니다: {id}");
                return false;
            }

            if (!data || !data.Clip)
                return false;

            return true;
        }

        private bool CanPlay(SoundData data, List<AudioSource> pool)
        {
            float lastTime;
            if (lastPlayTimes.TryGetValue(data.ID, out lastTime))
            {
                if (Time.unscaledTime < lastTime + data.Cooldown)
                    return false;
            }

            int playingCount = CountPlaying(data, pool);

            switch (data.OverlapPolicy)
            {
                case ESoundOverlapPolicy.AllowOverlap:
                    return true;

                case ESoundOverlapPolicy.RestartSame:
                    StopSame(data, pool);
                    return true;

                case ESoundOverlapPolicy.IgnoreIfPlaying:
                    return playingCount <= 0;

                case ESoundOverlapPolicy.LimitCount:
                    return playingCount < data.OverlapLimit;
            }

            return true;
        }

        private int CountPlaying(SoundData data, List<AudioSource> pool)
        {
            int count = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].isPlaying && pool[i].clip == data.Clip)
                    count++;
            }

            return count;
        }

        private void StopSame(SoundData data, List<AudioSource> pool)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].isPlaying && pool[i].clip == data.Clip)
                    pool[i].Stop();
            }
        }

        private AudioSource GetPlayableSource(List<AudioSource> pool)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].isPlaying)
                    return pool[i];
            }

            AudioSource source = CreateSource($"Extra_Source_{pool.Count}");
            pool.Add(source);

            return source;
        }

        private float GetFinalVolume(SoundData data)
        {
            float categoryVolume = 1f;

            switch (data.Category)
            {
                case ESoundCategory.BGM:
                    categoryVolume = bgmVolume;
                    break;

                case ESoundCategory.SFX:
                    categoryVolume = sfxVolume;
                    break;

                case ESoundCategory.UI:
                    categoryVolume = uiVolume;
                    break;

                case ESoundCategory.Ambient:
                    categoryVolume = ambientVolume;
                    break;
            }

            return data.Volume * categoryVolume * masterVolume;
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            RefreshBgmVolume();
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            RefreshBgmVolume();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
        }

        public void SetUiVolume(float value)
        {
            uiVolume = Mathf.Clamp01(value);
        }

        public void SetAmbientVolume(float value)
        {
            ambientVolume = Mathf.Clamp01(value);
        }

        private void RefreshBgmVolume()
        {
            if (!bgmSource || !bgmSource.clip)
                return;

            bgmSource.volume = bgmVolume * masterVolume;
        }
    }
}