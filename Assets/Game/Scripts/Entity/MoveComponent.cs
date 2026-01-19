using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveComponent : MonoBehaviour
    {
        public enum EMoveState
        {
            LEFT, RIGHT, STOP
        }

        protected Rigidbody2D rb = null;
        protected SpriteRenderer sr = null;

        [Header("일반")]
        [SerializeField]
        [Tooltip("중력")]
        protected float gravityScale = 10f;
        protected bool isGravityEnabled = true;

        protected bool canControl = true;
        public bool CanControl
        {
            get => canControl;
            set => canControl = value;
        }


        [Header("이동")]
        [SerializeField]
        [Tooltip("최대 속도")]
        protected float speedMax = 3.0f;
        [SerializeField]
        [Tooltip("가속력")]
        protected float acceleration = 6.0f;
        [SerializeField]
        [Tooltip("감속력")]
        protected float friction = 1.0f;

        private float hSpeed = 0f; // 현재 수평 속도
        private EMoveState direction = EMoveState.RIGHT;
        private EMoveState moveState = EMoveState.STOP;

        public float HSpeed => hSpeed;
        public float VSpeed => rb.velocity.y;
        public EMoveState Direction => direction;
        public EMoveState MoveState => moveState;

        [Header("벽판정 - 분홍색 박스로 표시됨")]
        [SerializeField]
        [Tooltip("벽으로 판정할 레이어")]
        private LayerMask wallLayer = default;
        [SerializeField]
        [Tooltip("벽 판정 박스")]
        private Vector2 wallBox = Vector2.zero;
        [SerializeField]
        [Tooltip("벽 판정 박스 Y 오프셋")]
        private float wallBoxYOffset = 0f;

        [Header("바닥 판정 - 푸른색 박스로 표시됨")]
        [SerializeField]
        [Tooltip("바닥으로 판정할 레이어")]
        private LayerMask groundLayer = default;
        [SerializeField]
        [Tooltip("바닥 판정 박스")]
        private Vector2 groundBox = Vector2.zero;
        [Tooltip("바닥 판정 박스 Y 오프셋")]
        [SerializeField]
        private float groundBoxYOffset = 0f;
        [Tooltip("바닥 체크 최대 거리")]
        [SerializeField]
        private float groundDistanceMax = 0.05f;

        [SerializeField]
        protected bool isGrounded = false; // 바닥인지 여부
        public bool IsGrounded => isGrounded;

        [Header("점프")]
        [SerializeField]
        [Tooltip("점프 파워")]
        protected float jumpPower = 10.0f;
        protected bool isJumping = false;
        protected bool canJump = false;


        private void Awake()
        {
            if (!TryGetComponent(out rb))
            {
                GameManager.Logger.LogError($"{gameObject.name}의 MoveComponent가 RIgidbody2D를 초기화하지 못했습니다.");
                enabled = false;
                return;
            }

            TryGetComponent(out sr);

            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.angularDrag = 0;
            rb.gravityScale = gravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        protected virtual void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateHSpeed(deltaTime);
            UpdateGrounded(deltaTime);
        }

        #region 이동 연산

#if UNITY_EDITOR

        /// <summary> 충돌 박스 표시 </summary>
        private void OnDrawGizmosSelected()
        {
            // 바닥 판정 상자
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Vector2 center = transform.position;
            center.y += groundBoxYOffset;
            Gizmos.DrawCube(center, groundBox);

            // 벽 판정 상자
            Gizmos.color = new Color(1, 0, 1, 0.5f);
            center = transform.position;
            center.y += wallBoxYOffset;
            Gizmos.DrawCube(center, wallBox);
        }

#endif

        /// <summary> 수평 속도 연산 </summary>
        private void UpdateHSpeed(float dt)
        {
            // 속도 변화량
            float speedDelta = acceleration * dt;
            bool calcFriction = false;

            // 최대 이동속도 제한
            switch (moveState)
            {
                case EMoveState.LEFT:
                    if (-speedMax < hSpeed)
                        hSpeed = Mathf.Max(-speedMax, hSpeed - speedDelta);
                    else
                        calcFriction = true;
                    break;
                case EMoveState.RIGHT:
                    if (hSpeed < speedMax)
                        hSpeed = Mathf.Min(speedMax, hSpeed + speedDelta);
                    else
                        calcFriction = true;
                    break;
                case EMoveState.STOP: // 정지했다면 마찰력 적용
                    calcFriction = true;
                    break;
            }

            if (calcFriction)
                hSpeed = Mathf.Max(0, Mathf.Abs(hSpeed) - friction * dt) * Mathf.Sign(hSpeed);

            // 벽에 닿았다면 속도 0으로 초기화
            Vector2 center = transform.position;
            center.y += wallBoxYOffset;
            if (Physics2D.BoxCast(center, wallBox, 0, Vector2.right, hSpeed * dt, wallLayer))
                hSpeed = 0;

            // 계산된 속도 적용
            Vector2 velocity = rb.velocity;
            velocity.x = hSpeed;
            rb.velocity = velocity;

            // 방향 변경
            if (hSpeed > 0)
            {
                direction = EMoveState.RIGHT;
                Vector3 v = transform.localScale;
                v.x = math.abs(v.x);
                transform.localScale = v;
            }
            else if (hSpeed < 0)
            {
                direction = EMoveState.LEFT;
                Vector3 v = transform.localScale;
                v.x = -math.abs(v.x);
                transform.localScale = v;
            }
        }

        /// <summary> 바닥 판정 연산 </summary>
        private void UpdateGrounded(float dt)
        {
            Vector2 groundBoxCenter = transform.position;
            groundBoxCenter.y += groundBoxYOffset;

            if (!Physics2D.BoxCast(groundBoxCenter, groundBox, 0f, Vector2.down, groundDistanceMax, groundLayer))
                isGrounded = false;
            else if (rb.velocity.y <= 0.001f) // 오차 반영
            {
                isGrounded = true;
                isJumping = false;
            }

            canJump = isGrounded;
        }

        #endregion

        #region 입력

        /// <summary> 이동 상태 설정 </summary>
        public void SetMovement(EMoveState newState)
        {
            if (!canControl)
                return;

            moveState = newState;
        }

        public void TryJump(bool force = false)
        {
            if (!force)
            {
                if (!canControl)
                    return;

                if (!canJump)
                    return;
            }

            isJumping = true;

            // 수직 속도 지정
            Vector2 velocity = rb.velocity;
            velocity.y = jumpPower;
            rb.velocity = velocity;
        }

        public void SetHSpeed(float f)
        {
            hSpeed = f;
        }

        public void SetVSpeed(float f)
        {
            Vector2 velocity = rb.velocity;
            velocity.y = f;
            rb.velocity = velocity;
        }

        public void SetGravityEnabled(bool b)
        {
            rb.gravityScale = b ? gravityScale : 0;
            isGravityEnabled = b;
        }

        #endregion
    }
}