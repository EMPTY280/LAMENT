using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("�÷��̾�")]
        [SerializeField] private Player player;

         [Header("테스트용")]
        [SerializeField] private PlayerHealth playerHealth;

        // ===== �޺� =====

        // �޺��� ����� �� �ִ� �Է��� ����

        private ComboNode root;
        private ComboNode currNode;
        private LinkedList<EComboInputTypes> inputQueue; // �Է��� Ű ��⿭, TODO: ������ ����׿����ιۿ� �Ⱦ�

        private EComboInputTypes inputBuffer = EComboInputTypes.NONE; // ���Է� ����
        private float bufferTime = 0; // ���������� ���Է��� �ð�
        [Header("�޺�")]
        [Tooltip("���Է��� ���� ���ӵǴ°�")]
        [SerializeField]
        private float bufferDuration = 0.2f; // ���Է� ���� �ð�

        private bool isLocked = false; // �Է� ���� ����

#if UNITY_EDITOR

        [Header("DEBUG")]
        [SerializeField]
        private Text DEBUG_COMBO_TEXT;

        private void PrintCombo()
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
            // TODO: InputHandler�� ���߿� �ٸ��� ������ ��.

            // �޺� �Է�
            GetComboKeyBuffer();
            ProcessInput();

            // �̵� �Է�
            GetMoveInput();

             HandleHealthDebugKeys();

#if UNITY_EDITOR
            PrintCombo();
#endif
        }

        private void HandleHealthDebugKeys()
        {
            if (playerHealth == null)
                return;

            // 1: 데미지 1
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerHealth.TakeHit(1);
                Debug.Log($"[DEBUG] Hit: HP = {playerHealth.CurrentHp}/{playerHealth.CurrentMaxHp}");
            }

            // 2: 적에게 공격 적중 -> 위 게이지 소량 증가
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerHealth.OnAttackLanded();
                Debug.Log($"[DEBUG] AttackLanded: Stomach = {playerHealth.StomachCurr}");
            }

            // 3: 사지 섭취 -> 위 게이지 많이 증가
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerHealth.OnLimbConsumed();
                Debug.Log($"[DEBUG] LimbConsumed: Stomach = {playerHealth.StomachCurr}");
            }
        }
        
        #region �Է� ó��

        // �޺� �Է� �ޱ�
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

        // �޺� ���Է� �ޱ�
        private void GetComboKeyBuffer()
        {
            // �Է� ��� == ��ų ����߿��� ���Է� ���
            if (!isLocked)
                return;

            EComboInputTypes input = GetComboKey();

            // �Է��� ���ٸ� �н�
            if (input == EComboInputTypes.NONE)
                return;

            // �Էµ� Ű�� �ִٸ� ���Է� ���� �� �ð� ����
            inputBuffer = input;
            bufferTime = Time.time;
        }

        // �̵� �Է� �ޱ�
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
                player.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);
            else if (isLeftPressed)
                player.MoveComponent.SetMovement(MoveComponent.EDirection.LEFT);
            else
                player.MoveComponent.SetMovement(MoveComponent.EDirection.RIGHT);

            if (Input.GetKey(KeyCode.Space))
                player.MoveComponent.TryJump();
            else
                ((PlayerMoveComponent)player.MoveComponent).ForceEndJumping();
        }

        #endregion

        #region Build / Clear Player Combo

        // �޺� ��� ����
        private void ClearCombo()
        {
            root.ClearChild();
        }

        // �޺� �߰�
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

                    if (comboRoot == null) // ���� ���� ��Ʈ��
                        comboRoot = newNode;
                    else // �� �ܴ̿� ������� ���̱�
                        prevNode.AddChild(newNode);

                    prevNode = newNode;
                }

                if (comboRoot != null)
                    root.AddChild(comboRoot);

                // ������, ���� ��ų �߰�
                if (isWeapon)
                {
                    ComboNodeInput newNode = new(type + 3); // NOTE: Enum ��� ���� �س���
                    newNode.Set(slot, ((Weapon)slot.Equipment).BurstSkill, true);

                    root.AddChild(newNode);
                }
            }

            // �� �޺�
            BuildFromSlot(EComboInputTypes.LEFT, player.LeftArmSlot, true);
            BuildFromSlot(EComboInputTypes.RIGHT, player.RightArmSlot, true);

            // �ٸ� �޺�
            BuildFromSlot(EComboInputTypes.UTILITY, player.LegSlot);
        }

        #endregion

        #region �޺� Ž��

        // �޺� Ž��
        private void ProcessInput()
        {
            // �Է� ��� == ��ų ������̸� ���� ����
            if (isLocked)
                return;

            // �Է� �ޱ� ===========================================

            EComboInputTypes input = EComboInputTypes.NONE;
            if (Time.time <= bufferTime + bufferDuration &&
                inputBuffer != EComboInputTypes.NONE)
                input = inputBuffer; // ���Է��� �ִٸ� �װ� ���
            else
                input = GetComboKey(); // ���Է� ������ ��� �Է� �õ�

            // ��ų Ž�� ===========================================

            // �Է��� ���ų� �޺��� ������ ����
            if (input == EComboInputTypes.NONE || currNode.Children.Count == 0)
            {
                player.FinishSkill();
                EndComboSearch();
            }
            else // �Է��� �ִٸ� Ž�� ����
            {
                // ���� ��忡�� ������ �� �ִ� ��� Ȯ��
                ComboNodeInput next = (ComboNodeInput)currNode.Children.Find((c) => c.Type == input);

                // ������, �� ����� ��ų�� ���� �õ�
                if (next != null)
                {
                    // ���� ���� �� ��� ���� �� Lock
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
                else // ������ �޺� Ž�� ���� + ��ٿ�
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