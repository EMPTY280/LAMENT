using System;
using System.Collections.Generic;
using UnityEngine;

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


        private GameManager() { }


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
                if (handler == null) return;
                var key = typeof(TEvent);

                if (_handlers.TryGetValue(key, out var del))
                {
                    _handlers[key] = Delegate.Combine(del, handler);
                }
                else
                {
                    _handlers[key] = handler;
                }
            }

            public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
            {
                if (handler == null) return;
                var key = typeof(TEvent);

                if (_handlers.TryGetValue(key, out var del))
                {
                    var currentDel = Delegate.Remove(del, handler);
                    if (currentDel == null) _handlers.Remove(key);
                    else _handlers[key] = currentDel;
                }
            }

            public static void Publish<TEvent>(TEvent evt) where TEvent : IGameEvent
            {
                var key = typeof(TEvent);
                if (_handlers.TryGetValue(key, out var del))
                {
                    if (del is Action<TEvent> action) action.Invoke(evt);
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
    
        #region 플레이어

        public static class Player
        {
            private static GutData[] guts = new GutData[(int)EGutType._LENGTH];

            public static GutData GetGutData(EGutType type)
            {
                return guts[(int)type];
            }

            public static void SetGutData(EGutType type, GutData data)
            {
                guts[(int)type] = data;
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
    }
}