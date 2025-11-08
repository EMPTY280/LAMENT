using UnityEngine;

namespace LAMENT
{
    public class PlayerMoveComponent : MoveComponent
    {
        [Header("플레이어 - 바닥 판정")]
        [Tooltip("바닥 판정 완충 시간")]
        [SerializeField]
        protected float coyoteTime = 0.3f;
        protected float coyoteTimeCurr = 0.0f;

        [Header("플레이어 - 점프")]
        /*
        [SerializeField]
        [Tooltip("추가 점프 횟수")]
        protected int bonusJump = 1;
        protected int bonusJumpCurr = 1;
        protected bool canBonusJump = false;
        */
        [SerializeField]
        [Tooltip("점프 지속 중 중력 배율")]
        protected float jumpGravityMult = 0.5f;


        protected override void Update()
        {
            base.Update(); // 여기서 이동 연산

            float deltaTime = Time.deltaTime;
            UpdateCoyoteTime(deltaTime);
            UpdateGravityScale();
        }

        #region 추가 이동 연산

        /// <summary> 점프 판정 완충 시간 계산 </summary>
        private void UpdateCoyoteTime(float dt)
        {
            // 점프중이면 완충 시간 즉시 0으로 제거함
            if (isJumping)
            {
                coyoteTimeCurr = 0;
                return;
            }

            // 땅에 서있다면 완충시간 최대로 한 뒤 패스
            if (isGrounded)
            {
                coyoteTimeCurr = coyoteTime;
                return;
            }

            // 공중일 때, 완충 시간이 남아있다면 점프 가능
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

        #endregion

        /// <summary> 점프 판정 강제 종료 (중력 계산을 위해...) </summary>
        public void ForceEndJumping()
        {
            isJumping = false;
        }

        public void ResetCoyoteTime()
        {
            coyoteTimeCurr = 0;
        }
    }
}