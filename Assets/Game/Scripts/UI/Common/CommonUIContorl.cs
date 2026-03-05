using System;
using Unity.Mathematics;
using UnityEngine;


namespace LAMENT
{
    /// <summary> 상하좌우, 선택, 취소를 처리하는 클래스 </summary>
    public class CommonUIContorl : MonoBehaviour
    {
        [SerializeField] private bool isHInverted = false;
        [SerializeField] private bool isVInverted = false;

        public int HorizontalMove { private get; set; }
        public int VerticalMove { private get; set; }


        private int max = 1;
        private int pos = 0;

        public Action<int> CB_OnPositionMoved { private get; set; }
        public Action<int> CB_OnConfirmed { private get; set; }
        public Action<int> CB_OnCanceled { private get; set; }

        /// <summary> 최댓값 지정 </summary>
        public void SetMax(int maxIncluded)
        {
            if (maxIncluded <= 0)
                maxIncluded = 1;

            max = maxIncluded;

            if (max < pos)
                pos = max;
        }

        /// <summary> 위치 설정 </summary>
        public void SetPos(int p, bool isRelative, bool invokeCallback = true)
        {
            if (isRelative)
                pos += p;
            else
                pos = p;

            pos = math.clamp(pos, 0, max);

            if (invokeCallback && CB_OnPositionMoved != null)
                CB_OnPositionMoved(pos);
        }

        public int GetPos() => pos;
        
        private void Update()
        {
            bool up = Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.UP));
            bool down = Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.DOWN));
            bool left = Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.LEFT));
            bool right = Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.RIGHT));

            if (isHInverted)
            {
                bool temp = left;
                left = right;
                right = temp;
            }

            if (isVInverted)
            {
                bool temp = up;
                up = down;
                down = temp;
            }
            
            if (up)
                pos += VerticalMove;
            if (down)
                pos -= VerticalMove;
            if (left)
                pos -= HorizontalMove;
            if (right)
                pos += HorizontalMove;

            int ceiling = max + 1;
            pos = ((pos % ceiling) + ceiling) % ceiling;

            if (up || down || left || right)
            {
                if (CB_OnPositionMoved != null)
                    CB_OnPositionMoved(pos);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CONFIRM)))
            {
                if (CB_OnConfirmed != null)
                    CB_OnConfirmed(pos);
            }

            if (Input.GetKeyDown(GameManager.KeyMap.GetKeyCode(GameManager.KeyMap.EKey.CANCEL)))
            {
                if (CB_OnCanceled != null)
                    CB_OnCanceled(pos);
            }
        }
    }
}