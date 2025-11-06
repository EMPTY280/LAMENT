using UnityEngine;

public class GameManager
{
    #region ½Ì±ÛÅæ

    private static GameManager inst;

    private GameManager() { }

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
}