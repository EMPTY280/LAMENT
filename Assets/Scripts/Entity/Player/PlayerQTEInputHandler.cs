using System;
using UnityEngine;

namespace LAMENT
{
    internal sealed class PlayerQTEInputHandler
    {
        private readonly Player player;
        private readonly QTEManager qteManager;
        private readonly Action<ComboNodeInput, EComboInputTypes> onQteStarted;
        private readonly Action onLockRequested;
        private readonly Action onUnlockRequested;
        private readonly Action onBurstResolved;

        private ComboNodeInput pendingQteNode;
        private EComboInputTypes pendingQteInput = EComboInputTypes.NONE;

        public PlayerQTEInputHandler(
            Player player,
            QTEManager qteManager,
            Action<ComboNodeInput, EComboInputTypes> onQteStarted,
            Action onLockRequested,
            Action onUnlockRequested,
            Action onBurstResolved)
        {
            this.player = player;
            this.qteManager = qteManager;
            this.onQteStarted = onQteStarted;
            this.onLockRequested = onLockRequested;
            this.onUnlockRequested = onUnlockRequested;
            this.onBurstResolved = onBurstResolved;
        }

        public bool TryProcessQTEInput(bool isOverlayLocked)
        {
            if (isOverlayLocked)
                return true;

            if (qteManager == null || !qteManager.IsRunning)
                return false;

            EQTEDirection dir;

            if (!TryGetQTEDirectionDown(out dir))
                return true;

            qteManager.TryConsumeDirection(dir);
            return true;
        }

        public void NotifyDashExecuted()
        {
            if (qteManager == null)
                return;

            qteManager.NotifyDashExecuted();
        }

        public bool TryBeginQTE(ComboNodeInput next, EComboInputTypes input)
        {
            Debug.Log($"[QTE][INPUT] TryBeginQTE - slot: skill:{next.Skill.name} burst:{next.IsBurst}");

            if (qteManager == null)
                return false;

            if (next == null || next.Equipment == null || next.Equipment.Equipment == null || next.Skill == null)
                return false;

            EEquipSlotType slotType = next.Equipment.Type;

            if (slotType != EEquipSlotType.LEFT && slotType != EEquipSlotType.RIGHT)
                return false;

            bool isComboFinisher = next.Children == null || next.Children.Count == 0;

            bool started = qteManager.TryBegin(
                slotType,
                next.Equipment.Equipment,
                next.Skill,
                next.IsBurst,
                isComboFinisher,
                OnQTEFinished);

            Debug.Log($"[QTE][INPUT] TryBegin result = {started}");

            if (!started)
                return false;

            pendingQteNode = next;
            pendingQteInput = input;

            onQteStarted?.Invoke(next, input);
            onLockRequested?.Invoke();

            return true;
        }

        public void ClearPending()
        {
            pendingQteNode = null;
            pendingQteInput = EComboInputTypes.NONE;
        }

        private void OnQTEFinished(QTEResultContext context)
        {
            Debug.Log($"[QTE][INPUT] QTE Finished - success:{context.IsSuccess} mult:{context.DamageMultiplier}");

            if (pendingQteNode == null)
            {
                onUnlockRequested?.Invoke();
                return;
            }

            bool used = player.TryUseEquipment(
                pendingQteNode.Equipment,
                pendingQteNode.Skill,
                onUnlockRequested,
                pendingQteNode.IsBurst,
                context);

            if (!used)
                onUnlockRequested?.Invoke();

            if (pendingQteNode.IsBurst)
                onBurstResolved?.Invoke();

            ClearPending();
        }

        private static bool TryGetQTEDirectionDown(out EQTEDirection dir)
        {
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP)))
            {
                dir = EQTEDirection.Up;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN)))
            {
                dir = EQTEDirection.Down;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT)))
            {
                dir = EQTEDirection.Left;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT)))
            {
                dir = EQTEDirection.Right;
                return true;
            }

            dir = EQTEDirection.Up;
            return false;
        }
    }
}
