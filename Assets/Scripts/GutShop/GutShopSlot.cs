using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class GutShopSlot : MonoBehaviour
    {
       [Header("기본")]
       [SerializeField] private Image iconImg;
       [SerializeField] private Text priceTxt;
       [SerializeField] private GameObject focusObj;

       [Header("상태표시")]
       [SerializeField] private GameObject soldOutObj;
       [SerializeField] private Image soldOutImg;
       [SerializeField] private Text soldOutTxt;

       [Header("색상")]
       [SerializeField] private Color normalIconColor = Color.white;
        [SerializeField] private Color selectedIconColor = Color.white;
        [SerializeField] private Color soldOutIconColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color soldOutTextColor = Color.red;

        private GutShopItem item;
        private bool isSelected = false;
        private bool isSoldOut = false;

        public GutShopItem Item => item;

        public void SetItem(GutShopItem newItem)
        {
            item = newItem;

            bool hasItem = item != null && item.Gut != null;
            gameObject.SetActive(hasItem);

            if(!hasItem)
                return;
            if(iconImg)
                iconImg.sprite = item.Gut.Icon;
            if(priceTxt)
                priceTxt.text = item.Price.ToString();

            SetSoldOut(GameManager.GameUnlock.IsUnlocked(item.Gut.ID));
            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            isSelected = value;

            if(focusObj)
                focusObj.SetActive(value);

            RefreshVisual();
        }

        public void SetSoldOut(bool value)
        {
            isSoldOut = value;

            if(soldOutObj)
                soldOutObj.SetActive(value);
            
            if(soldOutImg)
                soldOutImg.enabled = value;
            
            if(soldOutTxt)
            {
                soldOutTxt.enabled = value;
                soldOutTxt.text = "SOLD OUT";
                soldOutTxt.color = soldOutTextColor;
            }
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if(!iconImg)
                return;

            if(isSoldOut)
                iconImg.color = soldOutIconColor;
            else if(isSelected)
                iconImg.color = selectedIconColor;
            else
                iconImg.color = normalIconColor;
        }
    }

}
