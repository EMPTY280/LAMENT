using System;
using UnityEngine;
using UnityEngine.UI;


namespace LAMENT
{
    public class GutsManager : MonoBehaviour
    {
        // ===== 슬롯 =====
        [Header("슬롯")]
        [SerializeField] private SpriteRenderer[] silhouettes;
        [SerializeField] private GutSlot[] gutSlots;

        [SerializeField] private GutSlot descPanel;

        [SerializeField] private Color silhouettDefault;
        [SerializeField] private Color silhouettFocused;
        [SerializeField] private Color silhouettEquipped;

        // ===== 커서 =====
        [Header("커서")]
        [SerializeField] private RectTransform cursorRect;
        private int gutCursor = 0;
        private int invCursor = 0;

        // ===== 인벤 =====
        [Serializable]
        private class GutList
        {
            public GutData[] list;
        }

        [Header("인벤")]
        [SerializeField] private GutList[] collections;
        [SerializeField] private Image[] invIcons;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private RectTransform equippedText;
        [SerializeField] private int invSlotPerLine = 3;

        private bool isInvMode = false;

        // ===== 설명 =====
        [Header("설명창")]
        [SerializeField] private Image descFrame;
        [SerializeField] private Image descIcon;
        [SerializeField] private Text descNameTxt;
        [SerializeField] private Text descTxt;
        [SerializeField] private string lockedTxt;
        [SerializeField] private Color lockedTxtColor;
        [SerializeField] private Color unlockedTxtColor;


        #region 초기화

        private void Awake()
        {
            foreach (GutSlot s in gutSlots)
                s.Clear();

            descPanel.Clear();

            for (int i = 0; i < silhouettes.Length; i++)
                silhouettes[i].color = GameManager.Player.GetGutData((EGutType)i) != null ? silhouettEquipped : silhouettDefault;
            
            MoveGutCursor(0, false);
            UpdateDescPanel();
            equippedText.gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            if (silhouettes == null)
                return;

            for (int i = 0; i < silhouettes.Length; i++)
                silhouettes[i].color = gutCursor == i ? silhouettFocused : silhouettDefault;
        }

        #endregion

        #region 업데이트

        private void Update()
        {
            UpdateCursorSize();
            UpdateSelectedGutColor();
            GetInput();
        }

        /// <summary> 커서 크기 조절 </summary>
        private void UpdateCursorSize()
        {
            float size = Mathf.Sin(Time.time * 10) * 2f;

            cursorRect.offsetMax = Vector2.one * (10 - size); // Right, Top
            cursorRect.offsetMin = -Vector2.one * (10 - size); // Left, Bottom
        }

        private void UpdateSelectedGutColor()
        {
            silhouettes[gutCursor].color = Color.Lerp(silhouettFocused, Color.white, 0.5f + 0.5f * Mathf.Sin(Time.time * 10));
        }

        #endregion

        #region 입력

        private void GetInput()
        {
            if (isInvMode)
            {
                if (Input.GetKeyDown(KeyCode.W) && 0 <= invCursor - invSlotPerLine)
                    MoveInvCursor(-invSlotPerLine, true);
                if (Input.GetKeyDown(KeyCode.S) && invCursor + invSlotPerLine < collections[gutCursor].list.Length)
                    MoveInvCursor(invSlotPerLine, true);
                if (Input.GetKeyDown(KeyCode.A))
                {
                    if ((invCursor + 1) % invSlotPerLine != 1)
                        MoveInvCursor(-1, true);
                    else
                        TryChangeMode(false);
                }
                if (Input.GetKeyDown(KeyCode.D) &&
                    ((invCursor + 1) % invSlotPerLine != 0 ||
                    invCursor + invSlotPerLine < collections[gutCursor].list.Length))
                    MoveInvCursor(1, true);
                if (Input.GetKeyDown(KeyCode.Space))
                    TryEquip();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.W))
                    MoveGutCursor(-1, true);
                if (Input.GetKeyDown(KeyCode.S))
                    MoveGutCursor(1, true);
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
                    TryChangeMode(true);
            }

            // TODO: 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameManager.GameUnlock.Unlock("heart_test_0");
                UpdateInventory();
                UpdateDescPanel();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameManager.GameUnlock.Unlock("heart_test_1");
                UpdateInventory();
                UpdateDescPanel();
            }
        }

        #endregion

        #region 커서

        private void MoveGutCursor(int v, bool isRelative)
        {
            int newPos = isRelative ? gutCursor + v : v;
            if (newPos < 0)
                newPos = (int)EGutType._LENGTH - 1;
            else if ((int)EGutType._LENGTH <= newPos)
                newPos = 0;

            silhouettes[gutCursor].color = GameManager.Player.GetGutData((EGutType)gutCursor) != null ? silhouettEquipped : silhouettDefault;
            gutCursor = newPos;
            silhouettes[gutCursor].color = silhouettFocused;

            SetCursorPos(gutSlots[gutCursor].transform);
            MoveInvCursor(0, false);
            UpdateInventory();
        }

        private void MoveInvCursor(int v, bool isRelative)
        {
            int newPos = isRelative ? invCursor + v : v;
            
            if (collections.Length <= 0)
                return;
            else
            {
                if (collections.Length <= gutCursor)
                    return;

                GutData[] list = collections[gutCursor].list;
                if (list != null)
                {
                    if (newPos < 0)
                        newPos = list.Length - 1;
                    else if (list.Length <= newPos)
                        newPos = 0;
                }
            }

            invCursor = newPos;

            if (isInvMode)
            {
                SetCursorPos(invIcons[invCursor].transform.parent);
                UpdateDescPanel();
            }
        }

        private void SetCursorPos(Transform t)
        {
            cursorRect.SetParent(t);
            UpdateCursorSize();
        }

        #endregion

        #region 모드

        private void TryChangeMode(bool isInvMode)
        {
            if (isInvMode &&
                (collections.Length <= gutCursor ||
                collections[gutCursor].list.Length <= 0))
                return;
            
            this.isInvMode = isInvMode;

            if (isInvMode)
                SetCursorPos(invIcons[invCursor].transform.parent);
            else
            {
                SetCursorPos(gutSlots[gutCursor].transform);
                MoveInvCursor(0, false);
            }
            UpdateDescPanel();
        }

        private void UpdateInventory()
        {
            if (collections.Length <= gutCursor)
            {
                for (int i = 0; i < invIcons.Length; i++)
                    invIcons[i].transform.parent.gameObject.SetActive(false);
                return;
            }
            
            equippedText.gameObject.SetActive(false);

            GutData[] list = collections[gutCursor].list;
            for (int i = 0; i < invIcons.Length; i++)
            {
                bool isActive = i < list.Length;
                invIcons[i].transform.parent.gameObject.SetActive(isActive);
                if (isActive)
                {
                    invIcons[i].sprite = GameManager.GameUnlock.IsUnlocked(list[i].ID) ? list[i].Icon : lockedIcon;
                    if (list[i] == GameManager.Player.GetGutData((EGutType)gutCursor))
                        SetEquippedText(invIcons[i].transform.parent);
                }
            }
        }

        #endregion

        #region 설명창

        private void UpdateDescPanel()
        {
            if (!isInvMode)
            {
                descFrame.enabled = false;
                descIcon.enabled = false;
                descNameTxt.enabled = false;
                descTxt.enabled = false;
                return;
            }

            GutData data = collections[gutCursor].list[invCursor];
            bool isUnlocked = GameManager.GameUnlock.IsUnlocked(data.ID);

            descFrame.enabled = true;

            descIcon.sprite = isUnlocked ? data.Icon : lockedIcon;
            descIcon.enabled = true;

            descNameTxt.text = isUnlocked ? data.Name : lockedTxt;
            descNameTxt.color = isUnlocked ? unlockedTxtColor : lockedTxtColor;
            descNameTxt.enabled = true;

            descTxt.text = isUnlocked ? data.Desc : data.UnlockMethod;
            descTxt.color = isUnlocked ? unlockedTxtColor : lockedTxtColor;
            descTxt.enabled = true;
        }

        #endregion
    
        #region 장착

        private void TryEquip()
        {
            // NOTE: 인덱스 예외처리는 커서 함수들에서 처리하기에 더 할 필요 없음.
            GutData selectedGut = collections[gutCursor].list[invCursor];
            if (!GameManager.GameUnlock.IsUnlocked(selectedGut.ID))
                return;

            GutData equippedGut = GameManager.Player.GetGutData((EGutType)gutCursor);
            if (equippedGut == selectedGut)
            {
                GameManager.Player.SetGutData((EGutType)gutCursor, null);
                gutSlots[gutCursor].Clear();
            }
            else
            {
                GameManager.Player.SetGutData((EGutType)gutCursor, selectedGut);
                gutSlots[gutCursor].SetData(selectedGut);
            }

            UpdateInventory();
        }
        
        private void SetEquippedText(Transform t)
        {
            equippedText.gameObject.SetActive(true);
            equippedText.SetParent(t);
            equippedText.offsetMax = Vector2.one * -8;
            equippedText.offsetMin = Vector2.one * 8;
        }

        #endregion
    }
}
