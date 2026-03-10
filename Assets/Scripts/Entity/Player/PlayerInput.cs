using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("플레이어")]
        [SerializeField] private Player player;

        // ===== 콤보 =====
        private ComboNode root;
        private ComboNode currNode;
        private LinkedList<EComboInputTypes> inputQueue;

        [Header("선입력")]
        [SerializeField]
        private float bufferDuration = 0.2f; // 선입력 유지 시간 (sec)
        private float bufferTime = 0; 
        private EComboInputTypes inputBuffer = EComboInputTypes.NONE;

        private bool isLocked = false;

#if UNITY_EDITOR

        [Header("DEBUG")]
        [SerializeField]
        private Text DEBUG_COMBO_TEXT;

        private void DEBUG_PrintCombo()
        {
            if (!DEBUG_COMBO_TEXT)
                return;

            string str = "Current Combo { ";

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
                str += "\n\n[Buffer]\n" + inputBuffer.ToString() + "\n";
                str += (bufferTime + bufferDuration - Time.time).ToString("F2") + " sec left";
            }
            else
                str += "\n\n[Buffer]\n" + "NONE\n";


            DEBUG_COMBO_TEXT.text = str;
        }

        private void DEBUG_Input()
        {
            // 대미지 1
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                player.OnHit(new()
                {
                    src = null,
                    amount = 1
                });
            }
            
            // 회복 1
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                player.SetHP(1, true);
            }

            // 위 게이지 소량 증가
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                player.SetEnergy(5, true);
            }

            // 위 게이지 대량 증가
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                player.SetEnergy(50, true);
            }

            /*
            // 3: 사지 섭취 -> 위 게이지 많이 증가
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerHealth.OnLimbConsumed();
                Debug.Log($"[DEBUG] LimbConsumed: Stomach = {playerHealth.StomachCurr}");
            }
            */
        }

#endif

        private void Start()
        {
            root = new ComboNode();
            currNode = root;
            inputQueue = new LinkedList<EComboInputTypes>();

            ClearCombo();
            BuildCombo();

            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        }

        void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        }

        private void Update()
        {
            HandleComboBuffer();
            ProcessInput();

            GetMoveInput();


#if UNITY_EDITOR
            DEBUG_PrintCombo();
            DEBUG_Input();
#endif
        }

        #region 콤보

        /// <summary> 현재 입력된 키 반환 </summary>
        private EComboInputTypes GetComboKey()
        {
            bool IsKeyPressed(GameManager.KeyMap.EKey type)
            {
                return Input.GetKey(GameManager.KeyMap.GetKeyCode(type));
            }

            if (IsKeyPressed(GameManager.KeyMap.EKey.SKILL_UTILITY))
                return EComboInputTypes.UTILITY;

            if (IsKeyPressed(GameManager.KeyMap.EKey.SKILL_PRIMARY))
                return EComboInputTypes.LEFT;

            if (IsKeyPressed(GameManager.KeyMap.EKey.SKILL_SECONDARY))
                return EComboInputTypes.RIGHT;

            if (IsKeyPressed(GameManager.KeyMap.EKey.BURST_PRIMARY))
                return EComboInputTypes.LEFT_BURST;

            if (IsKeyPressed(GameManager.KeyMap.EKey.BURST_SECONDARY))
                return EComboInputTypes.RIGHT_BURST;

            return EComboInputTypes.NONE;
        }

        /// <summary> 선입력 처리 </summary>
        private void HandleComboBuffer()
        {
            // 입력이 잠긴 동안에만 작동
            if (!isLocked)
                return;

            EComboInputTypes input = GetComboKey();

            if (input == EComboInputTypes.NONE)
                return;

            inputBuffer = input;
            bufferTime = Time.time;
        }

        /// <summary> 이동 처리 </summary>
        private void GetMoveInput()
        {
            bool IsKeyPressed(GameManager.KeyMap.EKey type)
            {
                return Input.GetKey(GameManager.KeyMap.GetKeyCode(type));
            }

            if (isLocked)
            {
                ((PlayerMoveComponent)player.MoveComponent).ForceEndJumping();
                ((PlayerMoveComponent)player.MoveComponent).ResetCoyoteTime();
                return;
            }

            bool isLeftPressed = IsKeyPressed(GameManager.KeyMap.EKey.MOVE_LEFT);
            bool isRightPressed = IsKeyPressed(GameManager.KeyMap.EKey.MOVE_RIGHT);

            if (isLeftPressed == isRightPressed)
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
            else
                player.MoveComponent.SetMovement(
                    isLeftPressed ? MoveComponent.EDirection.LEFT : MoveComponent.EDirection.RIGHT);

            if (IsKeyPressed(GameManager.KeyMap.EKey.JUMP))
            {
                if (player.MoveComponent.IsGrounded)
                    player.MoveComponent.TryJump();
                else if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.JUMP)))
                    player.MoveComponent.TryJump();
            }
            else
                (player.MoveComponent as PlayerMoveComponent).ForceEndJumping();
        }

        #endregion

        #region 콤보 생성

        /// <summary> 기존 콤보 제거 </summary>
        private void ClearCombo()
        {
            root.ClearChild();
        }

        /// <summary> 콤보 빌드 </summary>
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

                    if (comboRoot == null)
                        comboRoot = newNode;
                    else
                        prevNode.AddChild(newNode);

                    prevNode = newNode;
                }

                if (comboRoot != null)
                    root.AddChild(comboRoot);

                if (isWeapon)
                {
                    ComboNodeInput newNode = new(type + 3);
                    newNode.Set(slot, (slot.Equipment as WeaponData).BurstSkill, true);

                    root.AddChild(newNode);
                }
            }

            BuildFromSlot(EComboInputTypes.LEFT, player.LeftArmSlot, true);
            BuildFromSlot(EComboInputTypes.RIGHT, player.RightArmSlot, true);
            BuildFromSlot(EComboInputTypes.UTILITY, player.LegSlot);
        }

        #endregion

        #region 입력 처리

        private void ProcessInput()
        {
            if (isLocked)
                return;

            // 입력 처리 ===========================================

            EComboInputTypes input = EComboInputTypes.NONE;
            if (Time.time <= bufferTime + bufferDuration &&
                inputBuffer != EComboInputTypes.NONE)
                input = inputBuffer;
            else
                input = GetComboKey();

            // ===========================================

            if (input == EComboInputTypes.NONE || currNode.Children.Count == 0)
            {
                player.FinishSkill();
                EndComboSearch();
            }
            else
            {
                ComboNodeInput next = (ComboNodeInput)currNode.Children.Find((c) => c.Type == input);

                if (next != null)
                {
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
                else
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
            player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
            isLocked = true;
        }

        private void Unlock()
        {
            isLocked = false;
        }
    
        public void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
        {
            ClearCombo();
            BuildCombo();
        }
    }
}