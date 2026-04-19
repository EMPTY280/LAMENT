using System;
using UnityEngine;

namespace LAMENT
{
    public sealed class QTEManager : MonoBehaviour
    {
        [Header("DB")]
        [SerializeField] private QTEBindingDatabase bindingDatabase;

        private QTESession session;
        private Action<QTEResultContext> cbOnFinished;

        private float lastDashTime = -999f;
        private float prevTimeScale = 1f;
        private float prevFixedDeltaTime = 0.02f;

        public bool IsRunning => session != null && !session.IsFinished;

        private void Update()
        {
            if (!IsRunning)
                return;

            if (session.IsExpired())
                Finish(QTEResultContext.Fail, false);
        }

        public void NotifyDashExecuted()
        {
            lastDashTime = Time.time;
        }

        public bool TryBegin(
            EEquipSlotType slotType,
            EquipmentData equipment,
            Skill skill,
            bool isBurst,
            bool isComboFinisher,
            Action<QTEResultContext> cb)
        {
            Debug.Log($"[QTE][MANAGER] TryBegin called - slot:{slotType}, skill:{skill?.name}");
            if (IsRunning)
                return false;

            if (slotType != EEquipSlotType.LEFT && slotType != EEquipSlotType.RIGHT)
                return false;

            if (bindingDatabase == null)
                return false;

            if (!bindingDatabase.TryGet(slotType, equipment, skill, isBurst, out QTEData data))
            {
                 Debug.Log("[QTE][MANAGER] FAIL - DB match not found");
                  return false;
            }
               

            if (!CanTrigger(data, isComboFinisher))
            {
                Debug.Log($"[QTE][MANAGER] FAIL - Trigger condition not met ({data.TriggerType})");
                return false;
            }

            EQTEDirection[] sequence = BuildSequence(data);
            session = new QTESession(data, sequence);
            cbOnFinished = cb;

            StartSlow(data.SlowScale);

            GameManager.Eventbus.Publish(new GEOnQTEStarted(
                slotType,
                equipment,
                skill,
                sequence,
                data.TimeLimit,
                data.QteId));
             Debug.Log($"[QTE][MANAGER] SUCCESS - QTE Started ({data.QteId})");
            return true;
           
        }

        private bool CanTrigger(QTEData data, bool isComboFinisher)
        {
            switch (data.TriggerType)
            {
                case EQTETriggerType.ComboFinisher:
                    return isComboFinisher;

                case EQTETriggerType.AfterDash:
                    return Time.time <= lastDashTime + data.TriggerWindow;
            }

            return false;
        }

        private EQTEDirection[] BuildSequence(QTEData data)
        {
            if (data.SequenceMode == EQTESequenceMode.Fixed)
            {
                if (data.FixedSequence == null)
                    return Array.Empty<EQTEDirection>();

                EQTEDirection[] copied = new EQTEDirection[data.FixedSequence.Length];
                Array.Copy(data.FixedSequence, copied, copied.Length);
                return copied;
            }

            int len = Mathf.Max(1, data.RandomSequenceLength);
            EQTEDirection[] result = new EQTEDirection[len];

            for (int i = 0; i < len; i++)
                result[i] = (EQTEDirection)UnityEngine.Random.Range(0, 4);

            return result;
        }

        public bool TryConsumeDirection(EQTEDirection direction)
        {
            Debug.Log($"[QTE][INPUT] Direction input = {direction}");
            if (!IsRunning)
                return false;

            bool progressed;
            bool success;
            bool failed;

            session.TryInput(direction, out progressed, out success, out failed);

            if (progressed)
            {
                GameManager.Eventbus.Publish(new GEOnQTEProgress(
                    session.CurrentIndex,
                    session.Sequence.Length,
                    direction));
            }

            if (success)
            {
                QTEResultContext ctx = QTEResultContext.Success(
                    session.Data.SuccessDamageMultiplier,
                    session.Data.PreventBurstConsumeOnSuccess);

                Finish(ctx, true);
                return true;
            }

            if (failed)
            {
                Finish(QTEResultContext.Fail, false);
                return true;
            }

            return progressed;
        }

        private void Finish(QTEResultContext context, bool isSuccess)
        {
            if (session == null)
                return;

            EEquipSlotType slotType = EEquipSlotType.LEFT;
            EquipmentData equipped = null;
            Skill skill = null;
            string qteId = session.Data != null ? session.Data.QteId : string.Empty;

            StopSlow();

            if (isSuccess)
                GameManager.Eventbus.Publish(new GEOnQTESucceeded(qteId, context.DamageMultiplier, context.PreventBurstConsume));
                //Debug.Log("[QTE][RESULT] SUCCESS");
            else
                GameManager.Eventbus.Publish(new GEOnQTEFailed(qteId));
                Debug.Log("[QTE][RESULT] FAIL");

            GameManager.Eventbus.Publish(new GEOnQTEFinished(qteId, isSuccess));

            Action<QTEResultContext> callback = cbOnFinished;

            session = null;
            cbOnFinished = null;

            callback?.Invoke(context);
        }

        private void StartSlow(float slowScale)
        {
            prevTimeScale = Time.timeScale;
            prevFixedDeltaTime = Time.fixedDeltaTime;

            Time.timeScale = slowScale;
            Time.fixedDeltaTime = 0.02f * slowScale;
        }

        private void StopSlow()
        {
            Time.timeScale = prevTimeScale;
            Time.fixedDeltaTime = prevFixedDeltaTime;
        }
    }
}