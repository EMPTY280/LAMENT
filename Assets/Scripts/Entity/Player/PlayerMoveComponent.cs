using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class PlayerMoveComponent : MoveComponent
    {
        [Header("플레이어 - 바닥 판정")]
        [SerializeField, Tooltip("바닥 판정 완충 시간")] protected float coyoteTime = 0.3f;
        protected float coyoteTimeCurr = 0.0f;

        [Header("플레이어 - 점프")]
        [SerializeField, Tooltip("점프 지속 중 중력 배율")] protected float jumpGravityMult = 0.5f;
        [SerializeField, Tooltip("추가 점프 횟수")]protected int extraJump = 1;
        protected int extraJumpCurr = 1;
        protected bool canBonusJump = false;


        protected override void Update()
        {
            base.Update();

            float deltaTime = Time.deltaTime;
            UpdateCoyoteTime(deltaTime);
            UpdateGravityScale();
            UpdateExtraJump();
        }

        #region 추가 이동 연산

        /// <summary> 점프 판정 완충 시간 계산 </summary>
        private void UpdateCoyoteTime(float dt)
        {
            if (isJumping)
            {
                coyoteTimeCurr = 0;
                return;
            }

            if (isGrounded)
            {
                coyoteTimeCurr = coyoteTime;
                return;
            }

            if (coyoteTimeCurr > 0)
            {
                canJump = true;
                coyoteTimeCurr -= dt;
            }
        }

        /// <summary> 중력 조절 계산 </summary>
        private void UpdateGravityScale()
        {
            if (!isGravityEnabled)
                return;

            if (isJumping && rb.velocity.y > 0)
                rb.gravityScale = jumpGravityMult * gravityScale;
            else
                rb.gravityScale = gravityScale;
        }

        /// <summary> 추가 점프 연산 </summary>
        private void UpdateExtraJump()
        {
            if (isGrounded)
                extraJumpCurr = extraJump;

            if (0 < extraJumpCurr)
                canJump = true;
        }

        #endregion

        public override bool TryJump(bool force = false)
        {
            if (!isGrounded)
                extraJumpCurr--;

            return base.TryJump(force);
        }

        /// <summary> 점프 판정 강제 종료 (중력 계산을 위해.) </summary>
        public void ForceEndJumping()
        {
            isJumping = false;
        }

        public void ResetCoyoteTime()
        {
            coyoteTimeCurr = 0;
        }
    
        public void AddExtraJump(int value)
        {
            extraJump += value;
        }
    }
}

