using PLibrary;
using UnityEngine;

namespace LAMENT
{
    public class MonsterSpiderBoss : Entity
    {
        [SerializeField, Header("다리 찍기 구역")]
        private ZoneTriggerHandler zone1;
        [SerializeField]
        private ZoneTriggerHandler zone2;
        [SerializeField]
        private ZoneTriggerHandler zone3;
        [SerializeField]
        private ZoneTriggerHandler zone4;
        [SerializeField]
        private int currZone = 0;

        // ===== AI =====
        private BehaviorTree bt;

        private bool isStomping = false;
        private int stompZone = 0;
        private float lastStompTime = 0;
        [SerializeField, Header("AI")]
        private float stompInterval = 2f; // sec 단위

        // ===== 애니메이션 =====
        [SerializeField, Header("애니메이션")] private Animator mobAnimator;


        #region 초기화

        protected override void Awake()
        {
            base.Awake();

            InitZoneCallbacks();
            InitBT();
        }

        /// <summary> 플레이어가 어느 구역에 있는지 확인할 콜백 등록 </summary>
        private void InitZoneCallbacks()
        {
            zone1.CB_OnEnter = (zth, col) => { TrySetZone(col, 1); };
            zone2.CB_OnEnter = (zth, col) => { TrySetZone(col, 2); };
            zone3.CB_OnEnter = (zth, col) => { TrySetZone(col, 3); };
            zone4.CB_OnEnter = (zth, col) => { TrySetZone(col, 4); };
        }

        private void TrySetZone(Collider2D col, int zoneNum)
        {
            if (!col.CompareTag("Player"))
                return;
            
            currZone = zoneNum;
        }

        private void InitBT()
        {
            bt = new();

            BTSelectorNode root = new();
            bt.SetRootNode(root);

            BTSequenceNode stompSeq = new();
            root.AddChild(stompSeq);

            BTActionNode canStomp = new(CanStomp);
            stompSeq.AddChild(canStomp);

            BTActionNode startStomping = new(StartStomping);
            stompSeq.AddChild(startStomping);
        }

        #endregion

        #region 행동

        /// <summary> 찍기 공격이 가능한지 반환 </summary>
        private EBTState CanStomp()
        {
            if (isStomping)
                return EBTState.RUN;

            if (Time.time - lastStompTime < stompInterval)
                return EBTState.FAILURE;

            return EBTState.SUCCESS;
        }

        private EBTState StartStomping()
        {
            isStomping = true;
            stompZone = currZone;

            // 애니 실행, 자세한 타이밍은 애니메이션 클립에서 진행
            mobAnimator.SetTrigger($"Stomp{stompZone}");

            return EBTState.RUN;
        }

        /// <summary> 애니메이션 클립에서 호출 </summary>
        private void EndStomping()
        {
            isStomping = false;
            lastStompTime = Time.time;
        }

        #endregion

        protected override void Update()
        {
            base.Update();
            bt.Run();
        }
    }
}