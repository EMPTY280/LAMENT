using System;

namespace LAMENT
{
    [Serializable]
    public struct QTEResultContext
    {
        public bool IsTriggered;
        public bool IsSuccess;
        public float DamageMultiplier;
        public bool PreventBurstConsume;

        public static QTEResultContext None => new()
        {
            IsTriggered = false,
            IsSuccess = false,
            DamageMultiplier = 1f,
            PreventBurstConsume = false
        };

        public static QTEResultContext Fail => new()
        {
            IsTriggered = true,
            IsSuccess = false,
            DamageMultiplier = 1f,
            PreventBurstConsume = false
        };

        public static QTEResultContext Success(float damageMultiplier, bool preventBurstConsume)
        {
            return new QTEResultContext()
            {
                IsTriggered = true,
                IsSuccess = true,
                DamageMultiplier = damageMultiplier,
                PreventBurstConsume = preventBurstConsume
            };
        }
    }
}