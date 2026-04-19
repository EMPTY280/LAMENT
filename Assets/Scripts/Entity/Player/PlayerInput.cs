using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("플레이어")]
        [SerializeField] private Player player;

        [Header("QTE")]
        [SerializeField] private QTEManager qteManager;

        private ComboNode root;
        private ComboNode currNode;
        private LinkedList<EComboInputTypes> inputQueue;

        [Header("선입력")]
        [SerializeField]
        private float bufferDuration = 0.2f;
        private float bufferTime = 0;
        private EComboInputTypes inputBuffer = EComboInputTypes.NONE;

        private bool isLocked = false;

        private ComboNodeInput pendingQteNode = null;
        private EComboInputTypes pendingQteInput = EComboInputTypes.NONE;

#if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField]
        private Text DEBUG_COMBO_TEXT;
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

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        }

        private void Update()
        {
            if (TryProcessQTEInput())
                return;

            HandleComboBuffer();
            ProcessInput();
            GetMoveInput();

#if UNITY_EDITOR
            DEBUG_PrintCombo();
            DEBUG_Input();
#endif
        }

        #region QTE

        private bool TryProcessQTEInput()
        {
            if (qteManager == null || !qteManager.IsRunning)
                return false;

            EQTEDirection dir;
            if (!TryGetQTEDirectionDown(out dir))
                return true;

            qteManager.TryConsumeDirection(dir);
            return true;
        }

        private bool TryGetQTEDirectionDown(out EQTEDirection dir)
        {
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP)))
            {
                dir = EQTEDirection.Up;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN)))
            {
                dir = EQTEDirection.Down;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT)))
            {
                dir = EQTEDirection.Left;
                return true;
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT)))
            {
                dir = EQTEDirection.Right;
                return true;
            }

            dir = EQTEDirection.Up;
            return false;
        }

        public void NotifyDashExecuted()
        {
            if (qteManager == null)
                return;

            qteManager.NotifyDashExecuted();
        }

        private bool TryBeginQTE(ComboNodeInput next, EComboInputTypes input)
        {
            Debug.Log($"[QTE][INPUT] TryBeginQTE - slot: skill:{next.Skill.name} burst:{next.IsBurst}");
            if (qteManager == null)
                return false;

            if (next == null || next.Equipment == null || next.Equipment.Equipment == null || next.Skill == null)
                return false;

            EEquipSlotType slotType;
            if (!TryGetSlotType(next.Equipment, out slotType))
                return false;

            if (slotType != EEquipSlotType.LEFT && slotType != EEquipSlotType.RIGHT)
                return false;

            bool isComboFinisher = next.Children == null || next.Children.Count == 0;

            bool started = qteManager.TryBegin(
                slotType,
                next.Equipment.Equipment,
                next.Skill,
                next.IsBurst,
                isComboFinisher,
                OnQTEFinished);
                Debug.Log($"[QTE][INPUT] TryBegin result = {started}");

            if (!started)
                return false;

            pendingQteNode = next;
            pendingQteInput = input;

            currNode = next;
            inputQueue.AddFirst(input);
            Lock();

            return true;
        }

        private void OnQTEFinished(QTEResultContext context)
        {
            Debug.Log($"[QTE][INPUT] QTE Finished - success:{context.IsSuccess} mult:{context.DamageMultiplier}");
            if (pendingQteNode == null)
            {
                Unlock();
                return;
            }

            bool used = player.TryUseEquipment(
                pendingQteNode.Equipment,
                pendingQteNode.Skill,
                Unlock,
                pendingQteNode.IsBurst,
                context);

            if (!used)
                Unlock();

            if (pendingQteNode.IsBurst)
            {
                ClearCombo();
                BuildCombo();
            }

            pendingQteNode = null;
            pendingQteInput = EComboInputTypes.NONE;
        }

        private bool TryGetSlotType(EquipSlot slot, out EEquipSlotType slotType)
        {
            if (slot == player.LeftArmSlot)
            {
                slotType = EEquipSlotType.LEFT;
                return true;
            }

            if (slot == player.RightArmSlot)
            {
                slotType = EEquipSlotType.RIGHT;
                return true;
            }

            if (slot == player.LegSlot)
            {
                slotType = EEquipSlotType.LEG;
                return true;
            }

            slotType = EEquipSlotType.LEG;
            return false;
        }

        #endregion

        #region 콤보

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

        private void HandleComboBuffer()
        {
            if (!isLocked)
                return;

            EComboInputTypes input = GetComboKey();

            if (input == EComboInputTypes.NONE)
                return;

            inputBuffer = input;
            bufferTime = Time.time;
        }

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

        private void ClearCombo()
        {
            root.ClearChild();
        }

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

            EComboInputTypes input = EComboInputTypes.NONE;
            if (Time.time <= bufferTime + bufferDuration &&
                inputBuffer != EComboInputTypes.NONE)
                input = inputBuffer;
            else
                input = GetComboKey();

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
                    if (TryBeginQTE(next, input))
                        return;

                    if (player.TryUseEquipment(next.Equipment, next.Skill, Unlock, next.IsBurst, QTEResultContext.None))
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

#if UNITY_EDITOR
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
            DEBUG_COMBO_TEXT.text = str;
        }

        private void DEBUG_Input()
        {
        }
#endif
    }
}