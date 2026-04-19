using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class GutShopSceneManager : MonoBehaviour
    {
        [Header("상점 데이터")]
        [SerializeField] private GutShopData shopData;
        [SerializeField] private GutShopSlot[] slots;

        [Header(" 선택 이동")] 
        [SerializeField] private int[] moveLeft;
        [SerializeField] private int[] moveRight;
        [SerializeField] private int[] moveUp;
        [SerializeField] private int[] moveDown;

        [Header("설명창")]
        [SerializeField] private Image descIcon;
        [SerializeField] private Text descNameTxt;
        [SerializeField] private Text descTxt;
        [SerializeField] private Text descPriceTxt;
        [SerializeField] private Text descStateTxt;

        [Header("설명창 문구")]
        [SerializeField] private string soldOutTxt = "구매 완료";
        [SerializeField] private string cannotBuyTxt = "돈 부족";
        [SerializeField] private string canBuyTxt = "구매 가능";

        [Header("money")]
        [SerializeField] private Text moneyTxt;

        [Header("씬 이동")]
        [SerializeField] private string returnSceneName = "Worldmap";

        private int currIndex = 0;

        private void Awake()
        {
            InitSlots();
            RefreshMoney();
            SetFocus(currIndex);
            RefreshDesc();
        }

        private void OnEnable()
        {
            GameManager.Eventbus.Subscribe<GEOnMoneyChanged>(OnMoneyChanged);
        }

        private void OnDisable()
        {
            GameManager.Eventbus.Subscribe<GEOnMoneyChanged>(OnMoneyChanged);
        }

        private void Update()
        {
            ProcessMoveInput();
            ProcessConfirmInput();
            ProcessCancelInput();
        }

        private void InitSlots()
        {
            if (!shopData || shopData.Items == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                GutShopItem item = i < shopData.Items.Length ? shopData.Items[i] : null;

                if (slots[i])
                    slots[i].SetItem(item);
            }
        }

        private void OnMoneyChanged(GEOnMoneyChanged e)
        {
            RefreshMoney();
            RefreshDesc();
        }

        private void RefreshMoney()
        {
            if (!moneyTxt)
                return;

            moneyTxt.text = GameManager.Money.Get().ToString();
        }

        private void ProcessMoveInput()
        {
            int next = -1;

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT)))
                next = GetMoveIndex(moveLeft);
            else if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT)))
                next = GetMoveIndex(moveRight);
            else if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP)))
                next = GetMoveIndex(moveUp);
            else if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN)))
                next = GetMoveIndex(moveDown);

            if (next >= 0 && next < slots.Length && slots[next] && slots[next].gameObject.activeSelf)
                SetFocus(next);
        }

        private int GetMoveIndex(int[] table)
        {
            if (table == null)
                return -1;

            if (currIndex < 0 || currIndex >= table.Length)
                return -1;

            return table[currIndex];
        }

        private void SetFocus(int nextIndex)
        {
            if (nextIndex < 0 || nextIndex >= slots.Length)
                return;

            if (slots[currIndex])
                slots[currIndex].SetSelected(false);

            currIndex = nextIndex;

            if (slots[currIndex])
                slots[currIndex].SetSelected(true);

            RefreshDesc();
        }

        private void RefreshDesc()
        {
            if (currIndex < 0 || currIndex >= slots.Length)
                return;

            GutShopSlot slot = slots[currIndex];
            if (!slot || slot.Item == null || slot.Item.Gut == null)
                return;

            GutData gut = slot.Item.Gut;
            int price = slot.Item.Price;
            bool isUnlocked = GameManager.GameUnlock.IsUnlocked(gut.ID);
            bool canBuy = !isUnlocked && GameManager.Money.Get() >= price;

            if (descIcon)
            {
                descIcon.enabled = true;
                descIcon.sprite = gut.Icon;
                descIcon.color = isUnlocked ? new Color(0.45f, 0.45f, 0.45f, 1f) : Color.white;
            }

            if (descNameTxt)
            {
                descNameTxt.enabled = true;
                descNameTxt.text = gut.Name;
            }

            if (descTxt)
            {
                descTxt.enabled = true;
                descTxt.text = gut.Desc;
            }

            if (descPriceTxt)
            {
                descPriceTxt.enabled = true;
                descPriceTxt.text = price.ToString();
            }

            if (descStateTxt)
            {
                descStateTxt.enabled = true;

                if (isUnlocked)
                    descStateTxt.text = soldOutTxt;
                else if (!canBuy)
                    descStateTxt.text = cannotBuyTxt;
                else
                    descStateTxt.text = canBuyTxt;
            }
        }

        private void ProcessConfirmInput()
        {
            if (!Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CONFIRM)))
                return;

            TryBuyCurrent();
        }

        private void TryBuyCurrent()
        {
            if (currIndex < 0 || currIndex >= slots.Length)
                return;

            GutShopSlot slot = slots[currIndex];
            if (!slot || slot.Item == null || slot.Item.Gut == null)
                return;

            GutData gut = slot.Item.Gut;
            int price = slot.Item.Price;

            if (GameManager.GameUnlock.IsUnlocked(gut.ID))
                return;

            if (!GameManager.Money.TrySpend(price))
                return;

            GameManager.GameUnlock.Unlock(gut.ID);
            GameManager.Eventbus.Publish(new GEOnGutPurchased(gut, price));

            slot.SetSoldOut(true);
            RefreshDesc();
        }

        private void ProcessCancelInput()
        {
            if (!Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CANCEL)))
                return;

            ReturnScene();
        }

        public void ReturnScene()
        {
            GameManager.Instance.TryChangeScene(returnSceneName);
        }

    }

}

