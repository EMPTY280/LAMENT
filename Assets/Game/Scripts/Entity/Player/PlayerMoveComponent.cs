using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    public class PlayerMoveComponent : MoveComponent
    {
        [Header("플레이어 - 바닥 판정")]
        [Tooltip("바닥 판정 완충 시간")]
        [SerializeField] protected float coyoteTime = 0.3f;
        protected float coyoteTimeCurr = 0.0f;

        [Header("플레이어 - 점프")]
        [Tooltip("점프 지속 중 중력 배율")]
        [SerializeField] protected float jumpGravityMult = 0.5f;
        /*
        [SerializeField]
        [Tooltip("추가 점프 횟수")]
        protected int bonusJump = 1;
        protected int bonusJumpCurr = 1;
        protected bool canBonusJump = false;
        */

        // 장기/효과 등으로 추가되는 공중 점프 횟수
        private readonly Dictionary<object, int> extraJumpSources = new();
        private int extraJumpTotal = 0;
        private int extraJumpRemain = 0;

        protected override void Update()
        {
            base.Update();

            float deltaTime = Time.deltaTime;
            UpdateCoyoteTime(deltaTime);
            UpdateGravityScale();
            UpdateExtraJumpReset();
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

        /// <summary> 착지 시 추가 점프 횟수 초기화 </summary>
        private void UpdateExtraJumpReset()
        {
            if (isGrounded)
                extraJumpRemain = extraJumpTotal;
        }

        #endregion

        #region 추가 점프

        public void AddExtraJump(object source, int amount)
        {
            if (source == null)
                return;

            extraJumpSources[source] = Mathf.Max(0, amount);
            RecalculateExtraJump();

            Debug.Log($"[GUT][MOVE] AddExtraJump source={source}, total={extraJumpTotal}");
        }

        public void RemoveExtraJump(object source)
        {
            if (source == null)
                return;

            extraJumpSources.Remove(source);
            RecalculateExtraJump();

            Debug.Log($"[GUT][MOVE] RemoveExtraJump source={source}, total={extraJumpTotal}");
        }

        private void RecalculateExtraJump()
        {
            int total = 0;
            foreach (var kv in extraJumpSources)
                total += kv.Value;

            extraJumpTotal = Mathf.Max(0, total);

            if (isGrounded)
                extraJumpRemain = extraJumpTotal;
            else
                extraJumpRemain = Mathf.Min(extraJumpRemain, extraJumpTotal);
        }

        /// <summary>
        /// 기본 점프 + 공중 추가 점프 처리
        /// </summary>
        public bool TryJumpWithExtra()
        {
            // 기본 점프 가능 상태면 기존 점프 사용
            if (canJump)
            {
                TryJump();
                return true;
            }

            // 공중 추가 점프
            if (!isGrounded && extraJumpRemain > 0)
            {
                extraJumpRemain--;

                // 추가 점프는 수직속도를 한번 리셋해주는 편이 체감이 좋음
                rb.velocity = new Vector2(rb.velocity.x, 0f);

                canJump = true;
                isJumping = false;
                coyoteTimeCurr = 0f;

                TryJump();

                Debug.Log($"[GUT][MOVE] ExtraJump USED remain={extraJumpRemain}/{extraJumpTotal}");
                return true;
            }

            return false;
        }

        #endregion

        /// <summary> 점프 판정 강제 종료 (중력 계산을 위해.) </summary>
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

