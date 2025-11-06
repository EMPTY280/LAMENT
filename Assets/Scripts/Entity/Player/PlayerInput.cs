using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Player player;

    // ===== 콤보 =====

    // 콤보에 저장될 수 있는 입력의 종류

    private ComboNode root;
    private ComboNode currNode;
    private LinkedList<EComboInputTypes> inputQueue; // 입력한 키 대기열, TODO: 당장은 디버그용으로밖에 안씀

    private EComboInputTypes inputBuffer = EComboInputTypes.NONE; // 선입력 버퍼
    private float bufferTime = 0; // 마지막으로 선입력한 시간
    [Header("콤보")]
    [Tooltip("선입력이 몇초 지속되는가")]
    [SerializeField]
    private float bufferDuration = 0.2f; // 선입력 저장 시간

    private bool isLocked = false; // 입력 정지 여부

#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField]
    private Text DEBUG_COMBO_TEXT;

    private void PrintCombo()
    {
        if (!DEBUG_COMBO_TEXT)
            return;

        string str = "현재 콤보 { ";

        if (inputQueue.Count > 0)
        {
            LinkedListNode<EComboInputTypes> pointer = inputQueue.Last;

            while (pointer != null)
            {
                str += pointer.Value.ToString();

                if (pointer.Previous != null)
                    str += " - ";

                pointer = pointer.Previous;
            }
        }
        str += " }";

        if (Time.time <= bufferTime + bufferDuration)
        {
            str += "\n\n[선입력]\n" + inputBuffer.ToString() + "\n";
            str += (bufferTime + bufferDuration - Time.time).ToString("F2") + " sec left";
        }
        else
            str += "\n\n[선입력]\n" + "NONE\n";


        DEBUG_COMBO_TEXT.text = str;
    }

#endif


    private void Start()
    {
        root = new ComboNode();
        currNode = root;
        inputQueue = new LinkedList<EComboInputTypes>();

        ClearCombo();
        BuildCombo();
    }

    private void Update()
    {
        // TODO: InputHandler를 나중에 바르게 구현할 것.

        // 콤보 입력
        GetComboKeyBuffer();
        ProcessInput();

        // 이동 입력
        GetMoveInput();

#if UNITY_EDITOR
        PrintCombo();
#endif
    }

    #region 입력 처리

    // 콤보 입력 받기
    private EComboInputTypes GetComboKey()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            return EComboInputTypes.UTILITY;
        else if (Input.GetKey(KeyCode.Z))
            return EComboInputTypes.LEFT;
        else if (Input.GetKey(KeyCode.X))
            return EComboInputTypes.RIGHT;
        else if (Input.GetKey(KeyCode.C))
            return EComboInputTypes.LEFT_BURST;
        else if (Input.GetKey(KeyCode.V))
            return EComboInputTypes.RIGHT_BURST;

        return EComboInputTypes.NONE;
    }

    // 콤보 선입력 받기
    private void GetComboKeyBuffer()
    {
        // 입력 잠금 == 스킬 사용중에만 선입력 사용
        if (!isLocked)
            return;

        EComboInputTypes input = GetComboKey();

        // 입력이 없다면 패스
        if (input == EComboInputTypes.NONE)
            return;

        // 입력된 키가 있다면 선입력 저장 후 시간 갱신
        inputBuffer = input;
        bufferTime = Time.time;
    }

    // 이동 입력 받기
    private void GetMoveInput()
    {
        if (isLocked)
        {
            ((PlayerMoveComponent)player.MoveComponent).ForceEndJumping();
            ((PlayerMoveComponent)player.MoveComponent).ResetCoyoteTime();
            return;
        }

        bool isLeftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool isRightPressed = Input.GetKey(KeyCode.RightArrow);

        if (isLeftPressed == isRightPressed)
            player.MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
        else if (isLeftPressed)
            player.MoveComponent.SetMovement(MoveComponent.EMoveState.LEFT);
        else
            player.MoveComponent.SetMovement(MoveComponent.EMoveState.RIGHT);

        if (Input.GetKey(KeyCode.Space))
            player.MoveComponent.TryJump();
        else
            ((PlayerMoveComponent)player.MoveComponent).ForceEndJumping();
    }

    #endregion

    #region 콤보 컨테이너

    // 콤보 모두 제거
    private void ClearCombo()
    {
        root.ClearChild();
    }

    // 콤보 추가
    public void BuildCombo()
    {
        void BuildFromSlot(EComboInputTypes type, EquipSlot slot, bool isWeapon = false)
        {
            if (!slot.Equipment)
                return;

            ComboNode comboRoot = null;
            ComboNode prevNode = null;

            Skill[] skills = slot.Equipment.Skills;

            for (int i = 0; i < skills.Length; i++)
            {
                ComboNodeInput newNode = new(type);
                newNode.Set(slot, skills[i]);

                if (comboRoot == null) // 최초 노드는 루트로
                    comboRoot = newNode;
                else // 그 이외는 순서대로 붙이기
                    prevNode.AddChild(newNode);

                prevNode = newNode;
            }

            if (comboRoot != null)
                root.AddChild(comboRoot);

            // 무기라면, 폭파 스킬 추가
            if (isWeapon)
            {
                ComboNodeInput newNode = new(type + 3); // NOTE: Enum 계산 대충 해놨음
                newNode.Set(slot, ((Weapon)slot.Equipment).BurstSkill, true);

                root.AddChild(newNode);
            }
        }

        // 팔 콤보
        BuildFromSlot(EComboInputTypes.LEFT, player.LeftArmSlot, true);
        BuildFromSlot(EComboInputTypes.RIGHT, player.RightArmSlot, true);

        // 다리 콤보
        BuildFromSlot(EComboInputTypes.UTILITY, player.LegSlot);
    }

    #endregion

    #region 콤보 탐색

    // 콤보 탐색
    private void ProcessInput()
    {
        // 입력 잠금 == 스킬 사용중이면 실행 안함
        if (isLocked)
            return;

        // 입력 받기 ===========================================

        EComboInputTypes input = EComboInputTypes.NONE;
        if (Time.time <= bufferTime + bufferDuration &&
            inputBuffer != EComboInputTypes.NONE)
            input = inputBuffer; // 선입력이 있다면 그것 사용
        else
            input = GetComboKey(); // 선입력 없으면 즉시 입력 시도

        // 스킬 탐색 ===========================================

        // 입력이 없거나 콤보가 끝나면 종료
        if (input == EComboInputTypes.NONE || currNode.Children.Count == 0)
        {
            player.FinishSkill();
            EndComboSearch();
        }
        else // 입력이 있다면 탐색 시작
        {
            // 현재 노드에서 연결할 수 있는 노드 확인
            ComboNodeInput next = (ComboNodeInput)currNode.Children.Find((c) => c.Type == input);

            // 있으면, 그 노드의 스킬을 실행 시도
            if (next != null)
            {
                // 실행 성공 시 노드 저장 및 Lock
                if (player.TryUseEquipment(next.Equipment, next.Skill, Unlock, next.IsBurst))
                {
                    currNode = next;
                    inputQueue.AddFirst(input);
                    Lock();

                    if (next.IsBurst)
                    {
                        ClearCombo();
                        BuildCombo();
                    }
                }
            }
            else // 없으면 콤보 탐색 종료 + 쿨다운
            {
                player.FinishSkill();
                EndComboSearch();
            }
        }
    }

    private void EndComboSearch()
    {
        inputQueue.Clear();
        currNode = root;
    }

    #endregion

    private void Lock()
    {
        player.MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
        isLocked = true;
    }

    private void Unlock()
    {
        isLocked = false;
    }
}
