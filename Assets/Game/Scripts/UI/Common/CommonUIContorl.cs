using System;
using UnityEngine;


namespace LAMENT
{
    /// <summary> 상하좌우, 선택, 취소를 처리하는 클래스 </summary>
    public class CommonUIContorl : MonoBehaviour
    {
        // 각각 가로 세로 최대 범위
        private int horizontalRange = 1;
        private int verticalRange = 1;

        private int posH = 0;
        private int posV = 0;

        public Action<int, int> CB_OnPositionMoved { private get; set; }
        public Action<int, int> CB_OnConfirmed;
        public Action<int, int> CB_OnCanceled;


        /// <summary> 좌우로 선택 가능한 범위를 지정 </summary>
        public void SetHorizontalRange(int max)
        {
            if (max <= 0)
                max = 1;

            horizontalRange = max;

            if (horizontalRange < posH)
                posH = horizontalRange;
        }

        /// <summary> 상하로 선택 가능한 범위를 지정 </summary>
        public void SetVeticalRange(int max)
        {
            if (max <= 0)
                max = 1;

            verticalRange = max;

            if (verticalRange < posV)
                posV = verticalRange;
        }

        private void Update()
        {
            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP)))
            {
                posV = (posV + 1) % verticalRange;
                if (CB_OnPositionMoved != null)
                    CB_OnPositionMoved(posH, posV);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN)))
            {
                posV = (posV - 1) % verticalRange;
                if (CB_OnPositionMoved != null)
                    CB_OnPositionMoved(posH, posV);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT)))
            {
                posH = (posH - 1) % horizontalRange;
                if (CB_OnPositionMoved != null)
                    CB_OnPositionMoved(posH, posV);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT)))
            {
                posH = (posH + 1) % horizontalRange;
                if (CB_OnPositionMoved != null)
                    CB_OnPositionMoved(posH, posV);
            }


            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CONFIRM)))
            {
                if (CB_OnConfirmed != null)
                    CB_OnConfirmed(posH, posV);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CANCEL)))
            {
                if (CB_OnCanceled != null)
                    CB_OnCanceled(posH, posV);
            }
        }
    }
}