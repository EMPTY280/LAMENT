using System;
using UnityEngine;
using UnityEngine.UI;


namespace LAMENT
{
    public class GutsManager : MonoBehaviour
    {
        [Header("입력")]
        [SerializeField] private CommonUIContorl control;
        [SerializeField] private CursorRect cursor;
        [SerializeField] private Button returnBtt;

        private int gutPosPrev = 0;
        private int gutPos = 0;
        private int invPos = 0;
        private bool isInvMode = false;

        // ===== 슬롯 =====
        [Header("슬롯")]
        [SerializeField] private SpriteRenderer[] silhouettes;
        [SerializeField] private GutSlot[] gutSlots;

        [SerializeField] private Color silhouettDefault;
        [SerializeField] private Color silhouettFocused;
        [SerializeField] private Color silhouettEquipped;

        // ===== 인벤 =====
        [Header("인벤")]
        [SerializeField] private GutList[] collections;
        [SerializeField] private Image[] invIcons;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private RectTransform equippedText;
        [SerializeField] private int invSlotPerLine = 3;

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
            // 장기 슬롯 초기화
            for (int i = 0; i < gutSlots.Length; i++)
            {
                GutData data = GameManager.Player.GetGutData((EGutType)i);
                if (data)
                    gutSlots[i].SetData(data);
                else
                    gutSlots[i].Clear();
            }

            // 모든 실루엣들 색상 초기화
            for (int i = 0; i < silhouettes.Length; i++)
                UpdateSilhouett(i);

            // 설명창 초기화
            UpdateDescPanel();

            // 장착 텍스트 비활성화
            equippedText.gameObject.SetActive(false);

            SetAsGutMode();
        }

        private void OnValidate()
        {
            if (silhouettes == null)
                return;

            for (int i = 0; i < silhouettes.Length; i++)
                silhouettes[i].color = (i == 0) ? silhouettFocused : silhouettDefault;
        }

        #endregion

        #region 업데이트

        private void Update()
        {
            // 선택된 장기 실루엣만 색상 업데이트
            UpdateSilhouett(gutPos);

#if UNITY_EDITOR
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
#endif
        }

        private void UpdateSilhouett(int idx)
        {
            if (idx < 0 || silhouettes.Length <= idx)
                return;

            SpriteRenderer img = silhouettes[idx];

            if (idx == gutPos)
                img.color = Color.Lerp(silhouettFocused, Color.white, 0.5f + 0.5f * Mathf.Sin(Time.time * 10));
            else
                img.color = GameManager.Player.GetGutData((EGutType)idx) != null ? silhouettEquipped : silhouettDefault;
        }

        private void UpdateInventory()
        {
            if (gutPos < 0 || gutSlots.Length <= gutPos)
                return;

            equippedText.gameObject.SetActive(false);
            
            GutData[] list = collections[gutPos].List;
            for (int i = 0; i < invIcons.Length; i++)
            {
                bool isActive = i < list.Length;
                invIcons[i].transform.parent.gameObject.SetActive(isActive);

                if (isActive)
                {
                    invIcons[i].sprite = GameManager.GameUnlock.IsUnlocked(list[i].ID) ? list[i].Icon : lockedIcon;
                    if (list[i] == GameManager.Player.GetGutData((EGutType)gutPos))
                        SetEquippedText(invIcons[i].transform.parent);
                }
            }
        }

        private void UpdateDescPanel()
        {
            GutData data = null;

            // 현재 모드에 따라 장기 정보 가져오기
            if (0 <= gutPos && gutPos < gutSlots.Length)
            {
                if (isInvMode)
                    data = collections[gutPos].List[invPos];
                else
                    data = GameManager.Player.GetGutData((EGutType)gutPos);
            }

            // 장기 정보가 없으면 설명 비활성화
            if (!data)
            {
                descFrame.enabled = false;
                descIcon.enabled = false;
                descNameTxt.enabled = false;
                descTxt.enabled = false;
                return;
            }

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

        #region 입력

        /// <summary> 장기 슬롯 선택 모드로 전환 </summary>
        private void SetAsGutMode()
        {
            isInvMode = false;

            control.SetMax(gutSlots.Length); // 마지막은 뒤로가기 버튼
            control.HorizontalMove = 0;
            control.VerticalMove = 1;

            control.CB_OnPositionMoved = OnGutCursorMoved;
            control.CB_OnConfirmed = OnGutConfirmed;
            control.CB_OnCanceled = OnGutCanceled;

            control.SetPos(gutPos, false);

            UpdateDescPanel();
        }

        /// <summary> 장기 슬롯 선택 입력 </summary>
        private void OnGutCursorMoved(int pos)
        {
            gutPosPrev = gutPos;
            gutPos = pos;

            if (gutPos == gutSlots.Length) // 뒤로가기
            {
                UpdateSilhouett(gutPosPrev);
                cursor.SetParent(returnBtt.transform);
            }
            else
            {
                UpdateSilhouett(gutPosPrev);
                UpdateSilhouett(gutPos);

                UpdateInventory();
                cursor.SetParent(gutSlots[gutPos].transform);
            }

            UpdateDescPanel();
        }

        /// <summary> 장기 슬롯 선택 </summary>
        private void OnGutConfirmed(int pos)
        {
            if (gutPos == gutSlots.Length) // 뒤로가기
                ReturnToWorldmap();
            else
            {
                if (collections[gutPos] != null && 0 < collections[gutPos].List.Length)
                    SetAsInvMode();
            }
        }

        /// <summary> 장기 슬롯 취소 </summary>
        private void OnGutCanceled(int pos)
        {
            control.SetPos(gutSlots.Length, false);
        }

        /// <summary> 인벤토리 모드로 전환 </summary>
        private void SetAsInvMode()
        {
            isInvMode = true;

            control.SetMax(collections[gutPos].List.Length - 1);
            control.HorizontalMove = 1;
            control.VerticalMove = invSlotPerLine;

            control.CB_OnPositionMoved = OnInvCursorMoved;
            control.CB_OnConfirmed = OnInvConfirmed;
            control.CB_OnCanceled = OnInvCanceled;

            control.SetPos(0, false);
            control.SetPos(0, false, false);
        }

        private void OnInvCursorMoved(int pos)
        {
            invPos = pos;
            
            cursor.SetParent(invIcons[invPos].transform.parent);
            UpdateDescPanel();
        }

        private void OnInvConfirmed(int pos)
        {
            GutData selectedGut = collections[gutPos].List[invPos];
            if (!GameManager.GameUnlock.IsUnlocked(selectedGut.ID))
                return;

            GutData equippedGut = GameManager.Player.GetGutData((EGutType)gutPos);
            if (equippedGut == selectedGut)
            {
                GameManager.Player.SetGutData((EGutType)gutPos, null);
                gutSlots[gutPos].Clear();
            }
            else
            {
                GameManager.Player.SetGutData((EGutType)gutPos, selectedGut);
                gutSlots[gutPos].SetData(selectedGut);
            }

            UpdateInventory();
        }

        private void OnInvCanceled(int pos)
        {
            SetAsGutMode();
        }

        #endregion

        #region 기타

        public void ReturnToWorldmap()
        {
            GameManager.Instance.TryChangeScene("Worldmap");
        }

        public void SelectGutSlot(int idx)
        {
            if (isInvMode)
                SetAsGutMode();
            
            control.SetPos(idx, false);
            OnGutConfirmed(idx);
        }

        public void SelectInvSlot(int idx)
        {
            if (!isInvMode)
                SetAsInvMode();
            
            control.SetPos(idx, false);
            OnInvConfirmed(idx);
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
