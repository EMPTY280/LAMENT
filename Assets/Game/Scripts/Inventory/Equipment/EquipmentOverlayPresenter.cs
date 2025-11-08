using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    /// <summary>
    /// EquipmentPreviewEvent를 받아 3개 섹션(왼/오/다리)의 이전/현재/다음 아이콘 상태를 갱신.
    /// </summary>
    public sealed class EquipmentOverlayPresenter : MonoBehaviour
    {
        [Header("LeftArm (좌)")]
        [SerializeField] private Image _leftPrev;
        [SerializeField] private Image _leftCurrent;
        [SerializeField] private Image _leftNext;

        [Header("RightArm (우)")]
        [SerializeField] private Image _rightPrev;
        [SerializeField] private Image _rightCurrent;
        [SerializeField] private Image _rightNext;

        [Header("Legs (하)")]
        [SerializeField] private Image _legsPrev;
        [SerializeField] private Image _legsCurrent;
        [SerializeField] private Image _legsNext;

        [Header("Optional")]
        [SerializeField] private TextMeshProUGUI _hint; // "Tab 홀드: 장비 변경 / Z:왼 X:오 Shift:다리"

        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentPreview>(OnPreview);
        }
        private void OnDestroy() => GameManager.Eventbus.Unsubscribe<GEOnEquipmentPreview>(OnPreview);

        private void OnPreview(GEOnEquipmentPreview e)
        {
            Debug.Log($"OnPreview received for slot: {e.Slot}");
            switch (e.Slot)
            {
                case EEquipSlotType.LEFT:
                    Apply(e.Slot, _leftPrev, _leftCurrent, _leftNext, e.Prev, e.Current, e.Next);
                    break;
                case EEquipSlotType.RIGHT:
                    Apply(e.Slot, _rightPrev, _rightCurrent, _rightNext, e.Prev, e.Current, e.Next);
                    break;
                case EEquipSlotType.LEG:
                    Apply(e.Slot, _legsPrev, _legsCurrent, _legsNext, e.Prev, e.Current, e.Next);
                    break;
            }
        }

        private void Apply(EEquipSlotType slot, Image prev, Image current, Image next, EquipmentData p, EquipmentData c, EquipmentData n)
        {
            Debug.Log($"Applying icons for {slot}: Prev='{(p ? p.name : "None")}', Current='{(c ? c.name : "None")}', Next='{(n ? n.name : "None")}'");
            SetIcon(prev, p ? p.Icon : null, p != null, 0.45f);
            SetIcon(current, c ? c.Icon : null, c != null, 1.00f);
            SetIcon(next, n ? n.Icon : null, n != null, 0.45f);
        }

        private void SetIcon(Image img, Sprite spr, bool on, float alpha)
        {
            if (!img)
            {
                Debug.LogWarning("SetIcon: Image component is null.");
                return;
            }
            Debug.Log($"SetIcon for '{img.name}': Sprite='{(spr ? spr.name : "None")}', Alpha='{alpha}'");
            img.sprite = spr;
            img.color = on ? new Color(1, 1, 1, alpha) : new Color(1, 1, 1, 0.12f);
        }
    }
}
