using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class GutSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Sprite lockedSprite;

        [SerializeField] private Text nameTxt;
        [SerializeField] private string defaultNameStr;
        [SerializeField] private Text descTxt;
        [SerializeField] private string defaultDescStr;
        [SerializeField] private Color defaultTxtColor;
        [SerializeField] private Color highlightedTxtColor;

        public void SetData(GutData data)
        {
            if (!data)
                return;

            bool isUnLocked = GameManager.GameUnlock.IsUnlocked(data.ID);

            icon.sprite = isUnLocked ? data.Icon : lockedSprite;
            icon.enabled = true;

            nameTxt.text = isUnLocked ? data.Name : defaultNameStr;
            nameTxt.color = isUnLocked ? highlightedTxtColor : defaultTxtColor;
            
            descTxt.text = isUnLocked ? data.Desc : data.UnlockMethod;
            descTxt.color = isUnLocked ? highlightedTxtColor : defaultTxtColor;
        }

        public void Clear()
        {
            icon.enabled = false;

            nameTxt.text =  defaultNameStr;
            nameTxt.color = defaultTxtColor;
            
            descTxt.text = defaultDescStr;
            descTxt.color = defaultTxtColor;
        }

        private void OnValidate()
        {
            Clear();
        }
    }
}