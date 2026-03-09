using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    /// <summary> 키 도움말 텍스트 </summary>
    public class KeyTooltip : MonoBehaviour
    {
        [Serializable]
        private struct KeyTooltipStruct
        {
            public GameManager.KeyMap.EKey[] keys;
            public string str;
        }

        [SerializeField] private CanvasGroup canvGrp;
        [SerializeField] private Text textL;
        [SerializeField] private Text textR;

        [Header("출력 대상")]
        [SerializeField] private KeyTooltipStruct[] leftTooltips;
        [SerializeField] private KeyTooltipStruct[] rightTooltips;

        private void Awake()
        {
            textL.text = "";
            foreach (KeyTooltipStruct kts in leftTooltips)
            {
                textL.text += "[ ";
                foreach (GameManager.KeyMap.EKey k in kts.keys)
                    textL.text += $"{GameManager.KeyMap.GetKeyName(k)} ";
                textL.text += "] ";

                textL.text += kts.str + "  ";
            }
            textL.text = textL.text.Substring(0, math.max(0, textL.text.Length - 1));

            textR.text = "";
            foreach (KeyTooltipStruct kts in rightTooltips)
            {
                textR.text += kts.str;

                textR.text += " [ ";
                foreach (GameManager.KeyMap.EKey k in kts.keys)
                    textR.text += $"{GameManager.KeyMap.GetKeyName(k)} ";
                textR.text += "] ";

                textR.text += kts.str + "  ";
            }
            textR.text = textR.text.Substring(0, math.max(0, textR.text.Length - 1));
        }

        private void Update()
        {
            canvGrp.alpha = 0.3f + math.abs(math.sin(Time.time * 2f)) * 0.7f;
        }
    }
}