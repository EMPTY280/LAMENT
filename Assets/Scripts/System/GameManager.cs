using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LAMENT
{
    public class GameManager
    {
        #region 싱글톤

        private static GameManager inst;

        public static GameManager Instance
        {
            get
            {
                if (inst == null)
                    inst = new();
                return inst;
            }
        }

        #endregion

        private ScreenFade screenFade;

        private string overlayPrevSceneName = string.Empty;
        private string overlaySceneName = string.Empty;
        private bool isOverlayOpened = false;

        private float defaultFixedDeltaTime = 0.02f;
        private float pausedFixedDeltaTime = 0.02f;

        private GameManager()
        {
            screenFade = GameObject.Instantiate(Resources.Load<GameObject>("System/SCENE_TRANSITION_CANV")).GetComponent<ScreenFade>();
            defaultFixedDeltaTime = Time.fixedDeltaTime;
            pausedFixedDeltaTime = defaultFixedDeltaTime;
        }

        #region 씬 전환

        public bool TryChangeScene(string name, float duration = 1)
        {
            return screenFade.TryStartFadeout(duration * 0.5f, () =>
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = defaultFixedDeltaTime;

                SceneManager.LoadScene(name);
                screenFade.TryStartFadein(duration * 0.5f);
            });
        }

        /// <summary>
        /// 현재 씬을 유지한 채 오버레이 씬(예: 상점)을 Additive 로드한다.
        /// </summary>
        public bool TryOpenOverlayScene(string name, float duration = 0.25f)
        {
            if (isOverlayOpened)
                return false;

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
                return false;

            overlayPrevSceneName = activeScene.name;
            overlaySceneName = name;

            return screenFade.TryStartFadeout(duration * 0.5f, () =>
            {
                SceneManager.LoadScene(name, LoadSceneMode.Additive);

                Scene loadedScene = SceneManager.GetSceneByName(name);
                if (loadedScene.IsValid())
                    SceneManager.SetActiveScene(loadedScene);

                isOverlayOpened = true;

                Time.timeScale = 0f;
                Time.fixedDeltaTime = pausedFixedDeltaTime;

                screenFade.TryStartFadein(duration * 0.5f);
            });
        }

        /// <summary>
        /// 현재 열려있는 오버레이 씬을 닫고 이전 스테이지 씬으로 복귀한다.
        /// </summary>
        public bool TryCloseOverlayScene(float duration = 0.25f)
        {
            if (!isOverlayOpened)
                return false;

            return screenFade.TryStartFadeout(duration * 0.5f, () =>
            {
                string closingScene = overlaySceneName;
                string returnScene = overlayPrevSceneName;

                Time.timeScale = 1f;
                Time.fixedDeltaTime = defaultFixedDeltaTime;

                if (!string.IsNullOrEmpty(closingScene))
                    SceneManager.UnloadSceneAsync(closingScene);

                Scene prevScene = SceneManager.GetSceneByName(returnScene);
                if (prevScene.IsValid())
                    SceneManager.SetActiveScene(prevScene);

                overlaySceneName = string.Empty;
                overlayPrevSceneName = string.Empty;
                isOverlayOpened = false;

                screenFade.TryStartFadein(duration * 0.5f);
            });
        }

        public bool IsOverlayOpened()
        {
            return isOverlayOpened;
        }

        public string GetCurrentOverlaySceneName()
        {
            return overlaySceneName;
        }

        public string GetOverlayReturnSceneName()
        {
            return overlayPrevSceneName;
        }

        #endregion

        #region 로그

        public static class Logger
        {
            public static void LogError(string msg)
            {
#if UNITY_EDITOR
                Debug.Log($"[ERROR]: {msg}");
#endif
            }
        }

        #endregion

        #region 이벤트

        public static class Eventbus
        {
            private static readonly Dictionary<Type, Delegate> _handlers = new();

            public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
            {
                if (handler == null)
                    return;

                var key = typeof(TEvent);

                if (_handlers.TryGetValue(key, out var del))
                    _handlers[key] = Delegate.Combine(del, handler);
                else
                    _handlers[key] = handler;
            }

            public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
            {
                if (handler == null)
                    return;

                var key = typeof(TEvent);

                if (_handlers.TryGetValue(key, out var del))
                {
                    var currentDel = Delegate.Remove(del, handler);
                    if (currentDel == null)
                        _handlers.Remove(key);
                    else
                        _handlers[key] = currentDel;
                }
            }

            public static void Publish<TEvent>(TEvent evt) where TEvent : IGameEvent
            {
                var key = typeof(TEvent);
                if (_handlers.TryGetValue(key, out var del))
                {
                    if (del is Action<TEvent> action)
                        action.Invoke(evt);
                }
            }
        }

        #endregion

        #region 게임 속도

        public static class TimeSlowdownController
        {
            private static float slowScale = 0.25f;
            private static float defaultFixed;
            private static bool isInit = false;

            public static void TryInit()
            {
                if (isInit)
                    return;

                isInit = true;

                GameManager.Eventbus.Subscribe<GEOnOverlayStateChanged>(OnOverlay);
                defaultFixed = Time.fixedDeltaTime;
            }

            private static void OnOverlay(GEOnOverlayStateChanged e)
            {
                if (e.isOpened)
                {
                    Time.timeScale = slowScale;
                    Time.fixedDeltaTime = defaultFixed * slowScale;
                }
                else
                {
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = defaultFixed;
                }
            }
        }

        #endregion

        #region 플레이어 장기 데이터

        public static class Player
        {
            private static GutData[] guts = new GutData[(int)EGutType._LENGTH];

            public static GutData GetGutData(EGutType type)
            {
                return guts[(int)type];
            }

            public static void SetGutData(EGutType type, GutData data)
            {
                int idx = (int)type;
                GutData old = guts[idx];

                if (old == data)
                    return;

                guts[idx] = data;

                GameManager.Eventbus.Publish(new GEOnGutEquipped(type, data, old));
                GameManager.Eventbus.Publish(new GEOnGutLoadoutChanged());
            }
        }

        #endregion

        #region 게임 언락 (도전과제)

        /// <summary> 게임 컨텐츠의 잠금 해제 여부를 담당 </summary>
        public static class GameUnlock
        {
            private static HashSet<string> unlockSet = new();

            /// <summary> 그 ID의 언락 여부 반환 </summary>
            public static bool IsUnlocked(string id)
            {
                return unlockSet.Contains(id);
            }

            /// <summary> 그 ID를 언락 </summary>
            public static void Unlock(string id)
            {
                unlockSet.Add(id);
            }
        }

        #endregion

        #region 돈

        public static class Money
        {
            private static int current = 0;

            public static int Get()
            {
                return current;
            }

            public static void Set(int amount)
            {
                int next = Mathf.Max(0, amount);
                int delta = next - current;

                if (delta == 0)
                    return;

                current = next;
                GameManager.Eventbus.Publish(new GEOnMoneyChanged(current, delta));
            }

            public static void Add(int amount)
            {
                if (amount <= 0)
                    return;

                current += amount;
                GameManager.Eventbus.Publish(new GEOnMoneyChanged(current, amount));
            }

            public static bool TrySpend(int amount)
            {
                if (amount <= 0)
                    return false;
                if (current < amount)
                    return false;

                current -= amount;
                GameManager.Eventbus.Publish(new GEOnMoneyChanged(current, -amount));
                return true;
            }

            public static void Clear()
            {
                if (current == 0)
                    return;

                int delta = -current;
                current = 0;
                GameManager.Eventbus.Publish(new GEOnMoneyChanged(current, delta));
            }
        }

        #endregion

        #region 키 설정

        public static class KeyMap
        {
            public enum EKey
            {
                UP, DOWN, LEFT, RIGHT,
                CONFIRM, CANCEL,

                MOVE_LEFT, MOVE_RIGHT, JUMP,
                SKILL_UTILITY, SKILL_PRIMARY, SKILL_SECONDARY, BURST_PRIMARY, BURST_SECONDARY
            }

            private static Dictionary<EKey, KeyCode> keyMap;
            private static Dictionary<KeyCode, string> keyNameConverts;

            static KeyMap()
            {
                keyMap = new()
                {
                    { EKey.UP, KeyCode.UpArrow },
                    { EKey.DOWN, KeyCode.DownArrow },
                    { EKey.LEFT, KeyCode.LeftArrow },
                    { EKey.RIGHT, KeyCode.RightArrow },

                    { EKey.CONFIRM, KeyCode.Z },
                    { EKey.CANCEL, KeyCode.X },

                    { EKey.MOVE_LEFT, KeyCode.LeftArrow },
                    { EKey.MOVE_RIGHT, KeyCode.RightArrow },
                    { EKey.JUMP, KeyCode.Space },

                    { EKey.SKILL_UTILITY, KeyCode.LeftShift },
                    { EKey.SKILL_PRIMARY, KeyCode.Z },
                    { EKey.SKILL_SECONDARY, KeyCode.X },
                    { EKey.BURST_PRIMARY, KeyCode.C },
                    { EKey.BURST_SECONDARY, KeyCode.V },
                };

                keyNameConverts = new()
                {
                    { KeyCode.UpArrow, "▲" },
                    { KeyCode.DownArrow, "▼" },
                    { KeyCode.LeftArrow, "◀" },
                    { KeyCode.RightArrow, "▶" },

                    { KeyCode.LeftShift, "SHIFT" }
                };
            }

            /// <summary> 그 행동을 수행하는 키 코드 반환 </summary>
            public static KeyCode GetKeyCode(EKey type)
            {
                return keyMap[type];
            }

            /// <summary> 그 행동을 수행하는 키의 이름 반환 </summary>
            public static string GetKeyName(EKey type)
            {
                if (keyNameConverts.ContainsKey(keyMap[type]))
                    return keyNameConverts[keyMap[type]];
                return keyMap[type].ToString();
            }
        }

        #endregion
    }
}