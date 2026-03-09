using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    /// <summary>
    /// Tab 홀드 → 장비창 오픈(슬라이드), Z/X/Shift로 왼/오/다리 후보 순환,
    /// Tab 업 → 현재 선택 장비 확정(스왑) 후 닫기.
    ///
    /// UX:
    /// - 인덱스 -1: 중앙에 '현재 장착품' 표시.
    /// - 후보가 없으면 좌/우 비움.
    /// - 후보가 1개면 '장착품 ↔ 후보' 왕복.
    /// - 후보 >= 2면 캐러셀(Prev/Next 인접), 마지막 후보 다음은 다시 -1(장착품)로 복귀.
    /// - 탭 업 시 인덱스 -1이면 유지, 0 이상이면 해당 후보 장착.
    /// - 후보 리스트는 '현재 장착품 제외' + 중복 제거(HashSet).
    /// </summary>
    public sealed class EquipmentOverlayController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InventoryService _inventory;    // InventoryService
        [SerializeField] private EquipmentLoadoutService _loadout;       // 장착 도메인
        [SerializeField] private PanelSlideAnimator _overlayAnimator;    // 장비창 루트(슬라이드)

        // 슬롯별 "교체 후보" 리스트(현재 장착품 제외)
        private readonly List<EquipmentData> _left  = new();
        private readonly List<EquipmentData> _right = new();
        private readonly List<EquipmentData> _legs  = new();

        // 인덱스: -1 == 현재 장착품, 0..N-1 == 후보
        private int _idxL = -1, _idxR = -1, _idxG = -1;

        private bool _open;
        private bool _dirty;

        private void OnEnable()
        {
            GameManager.Eventbus.Subscribe<GEOnInventorySlotChanged>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            GameManager.Eventbus.Unsubscribe<GEOnInventorySlotChanged>(OnInventoryChanged);
        }

        private void Start()
        {
            if (_overlayAnimator) _overlayAnimator.InstantHide();
        }

        private void Update()
        {
            // 열기
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _open = true;
                _overlayAnimator?.Show();
                GameManager.Eventbus.Publish(new GEOnOverlayStateChanged(true));

                RebuildCandidates();        // 인벤토리 기준 후보 재구성(현재 장착품 제외)
                ResetIndicesToEquipped();   // -1(현재 장착)에서 시작
                PublishAllPreviews();
            }

            if (!_open) return;

            // 열린 동안 인벤토리 변화 즉시 반영
            if (_dirty)
            {
                _dirty = false;
                RebuildCandidates();
                ResetIndicesToEquipped();
                PublishAllPreviews();
            }

            // 캐러셀 입력
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log("Z key pressed");
                Cycle(EEquipSlotType.LEFT,  _left,  ref _idxL);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("X key pressed");
                Cycle(EEquipSlotType.RIGHT, _right, ref _idxR);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                Debug.Log("Shift key pressed");
                Cycle(EEquipSlotType.LEG,     _legs,  ref _idxG);
            }

            // 닫기(확정)
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                ConfirmSelections();                  // 장착 확정(선택 없으면 유지)
                _open = false;
                _overlayAnimator?.Hide();
                GameManager.Eventbus.Publish(new GEOnOverlayStateChanged(false));
            }
        }

        private void OnInventoryChanged(GEOnInventorySlotChanged _)
        {
            if (_open) _dirty = true;
        }

        /// <summary>
        /// 후보 리스트 재구성: 인벤토리의 장비들만 모으되, 현재 장착품은 제외.
        /// 중복(SO 참조 기준)은 제거(HashSet).
        /// </summary>
        private void RebuildCandidates()
        {
            _left.Clear(); _right.Clear(); _legs.Clear();

            if (_inventory == null || _inventory.SlotCount <= 0) return;

            var curLeft  = _loadout ? _loadout.CurrentLeft  : null;
            var curRight = _loadout ? _loadout.CurrentRight : null;
            var curLegs  = _loadout ? _loadout.CurrentLeg  : null;

            var leftSet  = new HashSet<EquipmentData>();
            var rightSet = new HashSet<EquipmentData>();
            var legsSet  = new HashSet<EquipmentData>();

            for (int i = 0; i < _inventory.SlotCount; i++)
            {
                var st = _inventory.GetSlot(i);
                if (st.IsEmpty) continue;

                if (st.Item is EquipmentData eq)
                {
                    // 현재 장착품은 후보에서 제외
                    if (eq == curLeft || eq == curRight || eq == curLegs)
                        continue;

                    switch (eq.Slot)
                    {
                        case EEquipSlotType.LEFT:  leftSet.Add(eq);  break;
                        case EEquipSlotType.RIGHT: rightSet.Add(eq); break;
                        case EEquipSlotType.LEG:     legsSet.Add(eq);  break;
                    }
                }
            }

            _left.AddRange(leftSet);
            _right.AddRange(rightSet);
            _legs.AddRange(legsSet);

            Debug.Log($"Rebuilt candidates (excluding equipped): Left({_left.Count}), Right({_right.Count}), Legs({_legs.Count})");
        }

        /// <summary>
        /// 오버레이를 켤 때/재빌드 후 인덱스를 현재 장착 기준으로 초기화(-1).
        /// </summary>
        private void ResetIndicesToEquipped()
        {
            _idxL = -1;
            _idxR = -1;
            _idxG = -1;
        }

        /// <summary>
        /// 왕복 캐러셀:
        /// - 후보 0: 항상 -1 유지
        /// - 후보 1+:  -1 → 0 → 1 → ... → N-1 → -1 → 0 ...
        /// </summary>
        private void Cycle(EEquipSlotType slot, List<EquipmentData> list, ref int index)
        {
            int count = list.Count;

            if (count == 0)
            {
                // 후보 없음: 현재 장착만 표시 유지
                index = -1;
                PublishOne(slot, list, -1);
                return;
            }

            if (index < 0)
            {
                // 현재 장착 → 첫 후보
                index = 0;
            }
            else if (index >= 0 && index < count - 1)
            {
                // 후보 i → 후보 i+1
                index += 1;
            }
            else
            {
                // 마지막 후보 → 현재 장착으로 복귀
                index = -1;
            }

            Debug.Log($"Cycling {slot}. New index: {index} (count={count})");
            PublishOne(slot, list, index);
        }

        private void PublishAllPreviews()
        {
            PublishOne(EEquipSlotType.LEFT,  _left,  _idxL);
            PublishOne(EEquipSlotType.RIGHT, _right, _idxR);
            PublishOne(EEquipSlotType.LEG,     _legs,  _idxG);
        }

        /// <summary>
        /// 프리뷰 규칙:
        /// - index == -1 : Current=장착품, Prev=장착품(되돌이 표시), Next=(후보가 있으면 list[0], 없으면 장착품)
        /// - index >= 0  :
        ///     * 후보가 1개면 Current=후보0, Prev=장착품, Next=장착품(왕복 암시)
        ///     * 후보가 2개 이상이면 Prev/Next는 인접 후보(정상 캐러셀)
        /// </summary>
        private void PublishOne(EEquipSlotType slot, List<EquipmentData> list, int index)
        {
            var equipped = GetEquipped(slot);

            // 중앙이 장착품(-1)인 경우
            if (index < 0)
            {
                var cur = equipped;

                // "후보 없음"이면 좌우 모두 장착품(비활성 표시로 렌더될 수 있음)
                EquipmentData prevForPreview = equipped;
                EquipmentData nextForPreview = list.Count > 0 ? list[0] : equipped;

                GameManager.Eventbus.Publish(new GEOnEquipmentPreview(slot, cur, prevForPreview, nextForPreview, -1));
                return;
            }

            // 후보가 0인데 index>=0 방어
            if (list.Count == 0)
            {
                GameManager.Eventbus.Publish(new GEOnEquipmentPreview(slot, equipped, equipped, equipped, -1));
                return;
            }

            var curCandidate = list[index];

            if (list.Count == 1)
            {
                // 후보 1개: 장착품 ↔ 후보 왕복
                EquipmentData prevCandidate = equipped; // 되돌이
                EquipmentData nextCandidate = equipped; // next 없으면 장착품으로 복귀

                GameManager.Eventbus.Publish(new GEOnEquipmentPreview(slot, curCandidate, prevCandidate, nextCandidate, index));
                return;
            }

            // 후보 2개 이상: 정상 캐러셀
            var prevC = list[(index - 1 + list.Count) % list.Count];
            var nextC = list[(index + 1) % list.Count];

            GameManager.Eventbus.Publish(new GEOnEquipmentPreview(slot, curCandidate, prevC, nextC, index));
        }

        private EquipmentData GetEquipped(EEquipSlotType slot)
        {
            if (_loadout == null) return null;
            return slot switch
            {
                EEquipSlotType.LEFT  => _loadout.CurrentLeft,
                EEquipSlotType.RIGHT => _loadout.CurrentRight,
                EEquipSlotType.LEG => _loadout.CurrentLeg,
                _ => null
            };
        }

        /// <summary>
        /// 탭 업 시 확정:
        /// - 인덱스 -1이면 그대로 유지(아무것도 하지 않음).
        /// - 0 이상이면 해당 후보 장착.
        /// </summary>
        private void ConfirmSelections()
        {
            if (_loadout == null) return;

            if (_idxL >= 0 && _idxL < _left.Count)  _loadout.Equip(_left[_idxL]);
            if (_idxR >= 0 && _idxR < _right.Count) _loadout.Equip(_right[_idxR]);
            if (_idxG >= 0 && _idxG < _legs.Count)  _loadout.Equip(_legs[_idxG]);

            // 다음 세션 대비 초기화
            _idxL = _idxR = _idxG = -1;
        }
    }
}
