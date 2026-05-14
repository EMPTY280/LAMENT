using UnityEngine;

namespace LAMENT
{
    internal sealed class PlayerInputSoundController
    {
        private readonly Player player;
        private readonly string footstepSoundId;
        private readonly string jumpSoundId;
        private readonly string landSoundId;

        private bool wasGrounded;
        private bool wasMoving;

        public PlayerInputSoundController(
            Player player,
            string footstepSoundId,
            string jumpSoundId,
            string landSoundId)
        {
            this.player = player;
            this.footstepSoundId = footstepSoundId;
            this.jumpSoundId = jumpSoundId;
            this.landSoundId = landSoundId;

            if (player != null && player.MoveComponent != null)
                wasGrounded = player.MoveComponent.IsGrounded;
        }

        public void UpdateFootstepSound(bool isInputLocked)
        {
            if (player == null || player.MoveComponent == null)
                return;

            bool isMoving =
                !isInputLocked &&
                player.MoveComponent.IsGrounded &&
                Mathf.Abs(player.MoveComponent.HSpeed) > 0.1f;

            if (isMoving && !wasMoving)
                PlaySFX(footstepSoundId);

            if (!isMoving && wasMoving)
                StopSFX(footstepSoundId);

            wasMoving = isMoving;
        }

        public void UpdateLandSound()
        {
            if (player == null || player.MoveComponent == null)
                return;

            bool isGrounded = player.MoveComponent.IsGrounded;

            if (!wasGrounded && isGrounded)
                PlaySFX(landSoundId);

            wasGrounded = isGrounded;
        }

        public void PlayJumpSound()
        {
            PlaySFX(jumpSoundId);
        }

        public void StopFootstepIfMoving()
        {
            if (!wasMoving)
                return;

            StopSFX(footstepSoundId);
            wasMoving = false;
        }

        private void PlaySFX(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (!SoundManager.Instance)
                return;

            SoundManager.Instance.PlaySFX(id);
        }

        private void StopSFX(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (!SoundManager.Instance)
                return;

            SoundManager.Instance.StopSFX(id);
        }
    }
}
