using System.Collections.Generic;
using LAMENT;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Player player;
    [SerializeField] private MoveComponent moveComponent;

    [Header("스프라이트 렌더러")]
    [SerializeField] private SpriteRenderer legRenderer;
    [SerializeField] private SpriteRenderer torsoRenderer;
    [SerializeField] private SpriteRenderer rArmRenderer;
    [SerializeField] private SpriteRenderer lArmRenderer;

    [Header("다리")]
    [SerializeField] private Sprite legIdle;
    [SerializeField] private List<Sprite> legMove;
    [SerializeField] private Sprite legMidAir;
    [SerializeField] private float legBaseSpeed = 1f;
    private float legIdx = 0;

    [Header("상체")]
    [SerializeField] private Sprite torsoIdle;
    [SerializeField] private List<Sprite> torsoMove;
    [SerializeField] private List<Sprite> torsoAttackLeft;
    [SerializeField] private List<Sprite> torsoAttackRight;
    private Vector3 torsoInitialPos;
    private float torsoAnimTime = 0;

    [Header("팔")]
    // 0번 공격 선딜, 1번 공격 후딜, 2번 다른 팔 공격 선딜, 3번 다른 팔 공격 후딜
    [SerializeField] private List<Vector3> leftArmPosList;
    [SerializeField] private List<Vector3> rightArmPosList;

    // 이동시 팔 위치
    [SerializeField] private List<Vector3> leftArmMovePosList;
    [SerializeField] private List<Vector3> rightArmMovePosList;

    private Vector3 leftArmDefaultPos;
    private Vector3 rightArmDefaultPos;

    private Sprite leftArmSprIdle;
    private Sprite rightArmSprIdle;
    private List<Sprite> armDelaySprites = new();
    private List<Sprite> armAttackSprites = new();

    private float skillDelaySpeed = 0f; // 스킬 선딜 애니 재생 속도
    private float skillAnimSpeed = 0f;  // 스킬 애니 재생 속도

    private float armAnimTime = 0f;
    private bool isAttacking = false;   // 스킬 사용중 ?
    private bool isSkillDelay = true;   // 스킬 선딜레이 중 ?
    private bool isRightAttack = false; // 오른팔 공격 ?

    [Header("애니메이션")]
    [SerializeField] private float breathSpeed = 5f;
    [SerializeField] private float breathAmp = 0.05f;
    [SerializeField] private float breathArmOffset = 1f;
    [SerializeField] private float torsoMoveSpeed = 5f;
    private float breathAnimTime = 0f;

    private void Awake()
    {
        if (torsoRenderer != null)
            torsoInitialPos = torsoRenderer.transform.localPosition;

        if (lArmRenderer != null)
            leftArmDefaultPos = lArmRenderer.transform.localPosition;

        if (rArmRenderer != null)
            rightArmDefaultPos = rArmRenderer.transform.localPosition;

        GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        GameManager.Eventbus.Subscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedSkill);
    }

    private void OnDestroy()
    {
        GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        GameManager.Eventbus.Unsubscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedSkill);
    }

    private void Update()
    {
        if (moveComponent == null)
            return;

        float hSpeed = math.abs(moveComponent.HSpeed);

        bool isMidAir = !moveComponent.IsGrounded;
        bool isIdle = hSpeed <= 0.1f;

        if (isMidAir)
            UpdateLegMidAir();
        else
        {
            if (isIdle)
                UpdateLegIdle();
            else
                UpdateLegMove(hSpeed);
        }

        if (isAttacking)
            UpdateAttack();
        else
        {
            if (isMidAir)
                UpdateTorsoMidAir();
            else if (isIdle)
                UpdateTorsoIdle();
            else
                UpdateTorsoMove();
        }
    }

    #region 다리 업데이트

    private void UpdateLegMidAir()
    {
        if (legRenderer == null)
            return;

        legRenderer.sprite = legMidAir;
        legIdx = 0f;
    }

    private void UpdateLegIdle()
    {
        if (legRenderer == null)
            return;

        legRenderer.sprite = legIdle;
        legIdx = 0f;
    }

    private void UpdateLegMove(float hSpeed)
    {
        if (legRenderer == null)
            return;

        if (legMove == null || legMove.Count == 0)
            return;

        legRenderer.sprite = legMove[(int)legIdx];
        legIdx = (legIdx + Time.deltaTime * legBaseSpeed * hSpeed) % legMove.Count;
    }

    #endregion

    #region 상체 업데이트

    private void UpdateTorsoMidAir()
    {
        if (torsoRenderer != null)
        {
            torsoRenderer.sprite = torsoIdle;
            torsoRenderer.transform.localPosition = torsoInitialPos;
        }

        if (lArmRenderer != null)
            lArmRenderer.transform.localPosition = leftArmDefaultPos;

        if (rArmRenderer != null)
            rArmRenderer.transform.localPosition = rightArmDefaultPos;
    }

    private void UpdateTorsoIdle()
    {
        if (torsoRenderer == null)
            return;

        torsoRenderer.sprite = torsoIdle;

        breathAnimTime += Time.deltaTime * breathSpeed;

        // 상체 위치
        Vector3 newPos = torsoInitialPos;
        newPos.y += math.sin(breathAnimTime) * breathAmp;
        torsoRenderer.transform.localPosition = newPos;

        // 양팔 위치
        float armOffset = math.sin(breathAnimTime + breathArmOffset) * breathAmp;

        if (lArmRenderer != null)
        {
            newPos = leftArmDefaultPos;
            newPos.y += armOffset;
            lArmRenderer.transform.localPosition = newPos;
        }

        if (rArmRenderer != null)
        {
            newPos = rightArmDefaultPos;
            newPos.y += armOffset;
            rArmRenderer.transform.localPosition = newPos;
        }
    }

    private void UpdateTorsoMove()
    {
        if (torsoRenderer == null)
            return;

        if (torsoMove == null || torsoMove.Count == 0)
            return;

        int idx = (int)(Time.time * torsoMoveSpeed) % torsoMove.Count;
        torsoRenderer.sprite = torsoMove[idx];

        if (lArmRenderer != null && leftArmMovePosList != null && idx < leftArmMovePosList.Count)
            lArmRenderer.transform.localPosition = leftArmMovePosList[idx];

        if (rArmRenderer != null && rightArmMovePosList != null && idx < rightArmMovePosList.Count)
            rArmRenderer.transform.localPosition = rightArmMovePosList[idx];
    }

    #endregion

    #region 공격 (팔 + 상체) 업데이트

    private void UpdateAttack()
    {
        if (!isAttacking)
            return;

        if (isSkillDelay)
        {
            // 딜레이 애니 재생 완료인지 먼저 확인
            if (armDelaySprites == null || armDelaySprites.Count <= armAnimTime)
            {
                // 양 팔 위치 지정
                if (rArmRenderer != null && rightArmPosList != null && rightArmPosList.Count > (isRightAttack ? 1 : 3))
                    rArmRenderer.transform.localPosition = rightArmPosList[isRightAttack ? 1 : 3];

                if (lArmRenderer != null && leftArmPosList != null && leftArmPosList.Count > (isRightAttack ? 3 : 1))
                    lArmRenderer.transform.localPosition = leftArmPosList[isRightAttack ? 3 : 1];

                // 플래그 계산
                isSkillDelay = false;
                armAnimTime = 0f;
                torsoAnimTime = 0f;
                return;
            }

            // 양 팔 위치 지정
            if (rArmRenderer != null && rightArmPosList != null && rightArmPosList.Count > (isRightAttack ? 0 : 2))
                rArmRenderer.transform.localPosition = rightArmPosList[isRightAttack ? 0 : 2];

            if (lArmRenderer != null && leftArmPosList != null && leftArmPosList.Count > (isRightAttack ? 2 : 0))
                lArmRenderer.transform.localPosition = leftArmPosList[isRightAttack ? 2 : 0];

            // 스프라이트 지정
            if (torsoRenderer != null)
            {
                if (isRightAttack)
                {
                    if (torsoAttackRight != null && torsoAttackRight.Count > 0)
                        torsoRenderer.sprite = torsoAttackRight[0];
                }
                else
                {
                    if (torsoAttackLeft != null && torsoAttackLeft.Count > 0)
                        torsoRenderer.sprite = torsoAttackLeft[0];
                }
            }

            if (isRightAttack)
            {
                if (rArmRenderer != null && armDelaySprites.Count > (int)armAnimTime)
                    rArmRenderer.sprite = armDelaySprites[(int)armAnimTime];
            }
            else
            {
                if (lArmRenderer != null && armDelaySprites.Count > (int)armAnimTime)
                    lArmRenderer.sprite = armDelaySprites[(int)armAnimTime];
            }

            armAnimTime += Time.deltaTime * skillDelaySpeed;
        }
        else
        {
            // 애니 재생 완료인지 먼저 확인
            if (armAttackSprites == null || armAttackSprites.Count <= armAnimTime)
            {
                EndAttackAnimation();
                return;
            }

            if (isRightAttack)
            {
                if (rArmRenderer != null && armAttackSprites.Count > (int)armAnimTime)
                    rArmRenderer.sprite = armAttackSprites[(int)armAnimTime];

                if (torsoRenderer != null && torsoAttackRight != null && torsoAttackRight.Count > 0)
                {
                    int torsoIdx = Mathf.Min(torsoAttackRight.Count - 1, 1 + (int)torsoAnimTime);
                    torsoRenderer.sprite = torsoAttackRight[torsoIdx];
                }
            }
            else
            {
                if (lArmRenderer != null && armAttackSprites.Count > (int)armAnimTime)
                    lArmRenderer.sprite = armAttackSprites[(int)armAnimTime];

                if (torsoRenderer != null && torsoAttackLeft != null && torsoAttackLeft.Count > 0)
                {
                    int torsoIdx = Mathf.Min(torsoAttackLeft.Count - 1, 1 + (int)torsoAnimTime);
                    torsoRenderer.sprite = torsoAttackLeft[torsoIdx];
                }
            }

            armAnimTime += Time.deltaTime * skillAnimSpeed;
            torsoAnimTime += Time.deltaTime * 15f;
        }
    }

    private void EndAttackAnimation()
    {
        if (torsoRenderer != null)
        {
            torsoRenderer.sprite = torsoIdle;
            torsoRenderer.transform.localPosition = torsoInitialPos;
        }

        if (rArmRenderer != null)
        {
            rArmRenderer.sprite = rightArmSprIdle;
            rArmRenderer.transform.localPosition = rightArmDefaultPos;
        }

        if (lArmRenderer != null)
        {
            lArmRenderer.sprite = leftArmSprIdle;
            lArmRenderer.transform.localPosition = leftArmDefaultPos;
        }

        isAttacking = false;
        isSkillDelay = true;
        armAnimTime = 0f;
        torsoAnimTime = 0f;
    }

    #endregion

    #region 이벤트 핸들링

    private void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
    {
        switch (e.SlotType)
        {
            case EEquipSlotType.LEG:
                break;

            case EEquipSlotType.LEFT:
            {
                WeaponData weapon = e.Equipped as WeaponData;

                if (weapon == null)
                {
                    leftArmSprIdle = null;
                    if (lArmRenderer != null)
                        lArmRenderer.sprite = null;
                    return;
                }

                leftArmSprIdle = weapon.SprIdle;
                if (lArmRenderer != null)
                    lArmRenderer.sprite = leftArmSprIdle;
                break;
            }

            case EEquipSlotType.RIGHT:
            {
                WeaponData weapon = e.Equipped as WeaponData;

                if (weapon == null)
                {
                    rightArmSprIdle = null;
                    if (rArmRenderer != null)
                        rArmRenderer.sprite = null;
                    return;
                }

                rightArmSprIdle = weapon.SprIdle;
                if (rArmRenderer != null)
                    rArmRenderer.sprite = rightArmSprIdle;
                break;
            }
        }
    }

    private void OnPlayerUsedSkill(GEOnPlayerUsedEquiment e)
    {
        // 다리는 공격 애니 없음
        if (e.SlotType == EEquipSlotType.LEG)
            return;

        WeaponData weapon = e.Equipment as WeaponData;
        if (weapon == null)
        {
            Debug.LogWarning("[PlayerAnimator] WeaponData가 null이라 공격 애니메이션을 재생할 수 없습니다.");
            return;
        }

        if (e.Skill == null)
        {
            Debug.LogWarning("[PlayerAnimator] Skill이 null이라 공격 애니메이션을 재생할 수 없습니다.");
            return;
        }

        if (weapon.SprAttack == null || weapon.SprDelay == null)
        {
            Debug.LogWarning($"[PlayerAnimator] {weapon.name} 무기의 공격/선딜 스프라이트 리스트가 null입니다.");
            return;
        }

        if (weapon.SprAttack.Count == 0 || weapon.SprDelay.Count == 0)
        {
            GameManager.Logger.LogError($"{e.Skill.name} 스킬에 플레이어 애니메이션 스프라이트가 올바르게 지정되지 않았습니다.");
            return;
        }

        if (e.Skill.PlayerDelay <= 0f)
        {
            Debug.LogWarning($"[PlayerAnimator] {e.Skill.name}의 PlayerDelay가 0 이하입니다.");
            return;
        }

        if (e.Skill.Duration <= e.Skill.PlayerDelay)
        {
            Debug.LogWarning($"[PlayerAnimator] {e.Skill.name}의 Duration이 PlayerDelay보다 작거나 같습니다.");
            return;
        }

        // 상체 위치 복구
        UpdateTorsoMidAir();

        if (rArmRenderer != null)
            rArmRenderer.sprite = rightArmSprIdle;

        if (lArmRenderer != null)
            lArmRenderer.sprite = leftArmSprIdle;

        // 플래그 계산
        isAttacking = true;
        isSkillDelay = true;
        isRightAttack = e.SlotType == EEquipSlotType.RIGHT;
        armAnimTime = 0f;
        torsoAnimTime = 0f;

        // 스프라이트 목록과 속도 할당
        armAttackSprites = weapon.SprAttack;
        armDelaySprites = weapon.SprDelay;

        skillDelaySpeed = armDelaySprites.Count / e.Skill.PlayerDelay;
        skillAnimSpeed = armAttackSprites.Count / (e.Skill.Duration - e.Skill.PlayerDelay);
    }

    #endregion
}