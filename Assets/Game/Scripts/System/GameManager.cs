using System;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class GameManager
    {
        #region ½Ì±ÛÅæ

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



        private GameManager()
        {

        }

        #region ·Î±×

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

        #region ÀÌº¥Æ® °ü¸®ÀÚ

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

        #region ½Ã°£ Á¶Àý

        /// <summary> ÀåºñÃ¢ ¿ÀÇÂ/´ÝÈû¿¡ ¸ÂÃç Time.timeScale Á¶Á¤. </summary>
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
    }
}