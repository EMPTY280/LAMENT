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

        [Header("Sound")]
        [SerializeField] private string footstepSoundId = "SFX_PLAYER_MOVE";
        [SerializeField] private string jumpSoundId = "SFX_PLAYER_JUMP";
        [SerializeField] private string landSoundId = "SFX_PLAYER_JUMP_DOWN";

        private PlayerInputSoundController soundController;
        private PlayerMovementInputHandler movementInput;
        private PlayerInputLockState lockState;
        private PlayerQTEInputHandler qteInput;

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

            soundController = new PlayerInputSoundController(player, footstepSoundId, jumpSoundId, landSoundId);
            movementInput = new PlayerMovementInputHandler(player, soundController);
            lockState = new PlayerInputLockState(player, soundController);
            qteInput = new PlayerQTEInputHandler(
                player,
                qteManager,
                OnQTEStarted,
                lockState.LockSkill,
                lockState.UnlockSkill,
                RebuildCombo);

            ClearCombo();
            BuildCombo();

            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Subscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Unsubscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
        }

        private void Update()
        {
            if (lockState.IsOverlayLocked)
            {
                soundController.UpdateLandSound();
                return;
            }

            if (qteInput.TryProcessQTEInput(lockState.IsOverlayLocked))
                return;

            HandleComboBuffer();
            ProcessInput();
            movementInput.UpdateMovement(lockState.IsInputLocked);

            soundController.UpdateFootstepSound(lockState.IsInputLocked);
            soundController.UpdateLandSound();

#if UNITY_EDITOR
            DEBUG_PrintCombo();
            DEBUG_Input();
#endif
        }

        public void NotifyDashExecuted()
        {
            qteInput.NotifyDashExecuted();
        }

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
            if (lockState.IsOverlayLocked)
                return;

            if (!lockState.IsSkillLocked)
                return;

            EComboInputTypes input = GetComboKey();

            if (input == EComboInputTypes.NONE)
                return;

            inputBuffer = input;
            bufferTime = Time.time;
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

        private void RebuildCombo()
        {
            ClearCombo();
            BuildCombo();
        }

        #endregion

        #region 입력 처리

        private void ProcessInput()
        {
            if (lockState.IsOverlayLocked)
                return;

            if (lockState.IsSkillLocked)
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
                    if (qteInput.TryBeginQTE(next, input))
                        return;

                    if (player.TryUseEquipment(next.Equipment, next.Skill, lockState.UnlockSkill, next.IsBurst, QTEResultContext.None))
                    {
                        currNode = next;
                        inputQueue.AddFirst(input);
                        lockState.LockSkill();

                        if (next.IsBurst)
                            RebuildCombo();
                    }
                }
                else
                {
                    player.FinishSkill();
                    EndComboSearch();
                }
            }
        }

        private void OnQTEStarted(ComboNodeInput next, EComboInputTypes input)
        {
            currNode = next;
            inputQueue.AddFirst(input);
        }

        private void EndComboSearch()
        {
            inputQueue.Clear();
            currNode = root;
        }

        private void ClearCombatState()
        {
            EndComboSearch();
            inputBuffer = EComboInputTypes.NONE;
            bufferTime = 0f;
            qteInput.ClearPending();
        }

        #endregion

        private void OnOverlayStateChanged(GEOnOverlayStateChanged e)
        {
            lockState.ApplyOverlayState(e, ClearCombatState);
        }

        public void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
        {
            RebuildCombo();
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
