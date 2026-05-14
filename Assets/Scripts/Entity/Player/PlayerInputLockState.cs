using System;

namespace LAMENT
{
    internal sealed class PlayerInputLockState
    {
        private readonly Player player;
        private readonly PlayerInputSoundController soundController;

        public bool IsSkillLocked { get; private set; }
        public bool IsOverlayLocked { get; private set; }
        public bool IsInputLocked => IsSkillLocked || IsOverlayLocked;

        public PlayerInputLockState(Player player, PlayerInputSoundController soundController)
        {
            this.player = player;
            this.soundController = soundController;
        }

        public void LockSkill()
        {
            if (player != null && player.MoveComponent != null)
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);

            IsSkillLocked = true;
            soundController.StopFootstepIfMoving();
        }

        public void UnlockSkill()
        {
            IsSkillLocked = false;
        }

        public void ApplyOverlayState(GEOnOverlayStateChanged e, Action onOverlayOpened)
        {
            IsOverlayLocked = e.isOpened;

            if (player == null || player.MoveComponent == null)
                return;

            if (IsOverlayLocked)
            {
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
                player.MoveComponent.SetHSpeed(0f);
                player.MoveComponent.CanControl = false;
            }
            else
            {
                player.MoveComponent.CanControl = true;
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
                player.MoveComponent.SetHSpeed(0f);
            }

            if (!IsOverlayLocked)
                return;

            onOverlayOpened?.Invoke();

            if (player.MoveComponent is PlayerMoveComponent playerMove)
            {
                playerMove.ForceEndJumping();
                playerMove.ResetCoyoteTime();
            }

            soundController.StopFootstepIfMoving();
        }
    }
}
