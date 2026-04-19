using System;
using UnityEngine;

namespace LAMENT
{
    [Serializable]
    public sealed class QTESession
    {
        private readonly QTEData data;
        private readonly EQTEDirection[] sequence;

        private int currentIndex = 0;
        private float startedTime = 0f;
        private bool isFinished = false;

        public QTEData Data => data;
        public EQTEDirection[] Sequence => sequence;
        public int CurrentIndex => currentIndex;
        public float StartedTime => startedTime;
        public bool IsFinished => isFinished;

        public QTESession(QTEData data, EQTEDirection[] sequence)
        {
            this.data = data;
            this.sequence = sequence ?? Array.Empty<EQTEDirection>();
            startedTime = Time.unscaledTime;
        }

        public bool IsExpired()
        {
            return Time.unscaledTime >= startedTime + data.TimeLimit;
        }

        public bool TryInput(EQTEDirection input, out bool progressed, out bool success, out bool failed)
        {
            progressed = false;
            success = false;
            failed = false;

            if (isFinished)
                return false;

            if (sequence == null || sequence.Length == 0)
            {
                isFinished = true;
                success = true;
                return true;
            }

            if (sequence[currentIndex] == input)
            {
                currentIndex++;
                progressed = true;

                if (currentIndex >= sequence.Length)
                {
                    isFinished = true;
                    success = true;
                }

                return true;
            }

            if (data.FailOnWrongInput)
            {
                isFinished = true;
                failed = true;
                return true;
            }

            return false;
        }

        public void ForceFail()
        {
            isFinished = true;
        }
    }
}