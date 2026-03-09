using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class HeartIcon : MonoBehaviour
    {
        [SerializeField] private Image heartImage;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;

          public void SetVisible(bool visible)
        {
            if (heartImage != null)
                heartImage.gameObject.SetActive(visible);
        }

        public void SetFilled(bool filled)
        {
            if (heartImage == null) return;

            if (filled && fullSprite != null)
                heartImage.sprite = fullSprite;
            else if (!filled && emptySprite != null)
                heartImage.sprite = emptySprite;
        }

    }

}
