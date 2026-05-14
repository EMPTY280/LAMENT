using UnityEngine;

namespace LAMENT
{
    internal sealed class PlayerMovementInputHandler
    {
        private readonly Player player;
        private readonly PlayerInputSoundController soundController;

        public PlayerMovementInputHandler(Player player, PlayerInputSoundController soundController)
        {
            this.player = player;
            this.soundController = soundController;
        }

        public void UpdateMovement(bool isInputLocked)
        {
            if (player == null || player.MoveComponent == null)
                return;

            if (isInputLocked)
            {
                if (player.MoveComponent is PlayerMoveComponent playerMove)
                {
                    playerMove.ForceEndJumping();
                    playerMove.ResetCoyoteTime();
                }
                return;
            }

            bool isLeftPressed = IsKeyPressed(GameManager.KeyMap.EKey.MOVE_LEFT);
            bool isRightPressed = IsKeyPressed(GameManager.KeyMap.EKey.MOVE_RIGHT);

            if (isLeftPressed == isRightPressed)
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
            else
                player.MoveComponent.SetMovement(
                    isLeftPressed ? MoveComponent.EDirection.LEFT : MoveComponent.EDirection.RIGHT);

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.JUMP)))
            {
                if (player.MoveComponent.TryJump())
                    soundController.PlayJumpSound();
            }

            if (!IsKeyPressed(GameManager.KeyMap.EKey.JUMP) &&
                player.MoveComponent is PlayerMoveComponent playerMoveComponent)
            {
                playerMoveComponent.ForceEndJumping();
            }
        }

        private static bool IsKeyPressed(GameManager.KeyMap.EKey type)
        {
            return Input.GetKey(GameManager.KeyMap.GetKeyCode(type));
        }
    }
}
