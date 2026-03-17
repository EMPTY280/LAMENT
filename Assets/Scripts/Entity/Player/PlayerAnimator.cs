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
    [SerializeField] private List<Sprite> torsoAttackLeft;
    [SerializeField] private List<Sprite> torsoAttackRight;
    private Vector3 torsoInitialPos;
    private float torsoAnimTime = 0;

    // 0번 공격 선딜, 1번 공격 후딜, 2번 다른 팔 공격 선딜, 3번 다른 팔 공격 후딜
    [Header("팔")]
    [SerializeField] private List<Vector3> leftArmPosList;
    [SerializeField] private List<Vector3> rightArmPosList;
    private Vector3 leftArmDefaultPos;
    private Vector3 rightArmDefaultPos;

    private Sprite leftArmSprIdle;
    private Sprite rightArmSprIdle;
    private List<Sprite> armDelaySprites = new();
    private List<Sprite> armAttackSprites = new();

    private float skillDelaySpeed = 0; // 스킬 선딜 애니 재생 속도
    private float skillAnimSpeed = 0; // 스킬 애니 재생 속도

    private float armAnimTime = 0;
    private bool isAttacking = false; // 스킬 사용중 ?
    private bool isSkillDelay = true; // 스킬 선딜레이 중 ?
    private bool isRightAttack = false; // 오른팔 공격 ?

    [Header("애니메이션")]
    [SerializeField] private float breathSpeed = 5f;
    [SerializeField] private float breathAmp = 0.05f;
    [SerializeField] private float breathArmOffset = 1f;
    [SerializeField] private float breathSpeedMoving = 7f;
    [SerializeField] private float breathAmpMoving = 0.1f;
    private float breathAnimTime = 0;


    private void Awake()
    {
        torsoInitialPos = torsoRenderer.transform.localPosition;
        leftArmDefaultPos = lArmRenderer.transform.localPosition;
        rightArmDefaultPos = rArmRenderer.transform.localPosition;

        GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        GameManager.Eventbus.Subscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedSkill);
    }

    private void OnDestroy()
    {
        GameManager.Eventbus.Unsubscribe<GEOnPlayerUsedEquiment>(OnPlayerUsedSkill);
    }

    private void Update()
    {
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
                ResetTorsoPosition();
            else
                UpdateTorsoIdle(!isIdle);
        }
    }

    #region 다리 업데이트

    private void UpdateLegMidAir()
    {
        legRenderer.sprite = legMidAir;
        legIdx = 0;
    }

    private void UpdateLegIdle()
    {
        legRenderer.sprite = legIdle;
        legIdx = 0;
    }

    private void UpdateLegMove(float hSpeed)
    {
        legRenderer.sprite = legMove[(int)legIdx];

        legIdx = (legIdx + Time.deltaTime * legBaseSpeed * hSpeed) % legMove.Count;
    }

    #endregion

    #region 상체 업데이트

    private void ResetTorsoPosition()
    {
        torsoRenderer.transform.localPosition = torsoInitialPos;
        lArmRenderer.transform.localPosition = leftArmDefaultPos;
        rArmRenderer.transform.localPosition = rightArmDefaultPos;
    }

    private void UpdateTorsoIdle(bool isMoving)
    {
        if (isMoving)
            breathAnimTime += Time.deltaTime * breathSpeedMoving;
        else
            breathAnimTime += Time.deltaTime * breathSpeed;

        // 상체 위치
        Vector3 newPos = torsoInitialPos;
        if (isMoving)
            newPos.y += math.sin(breathAnimTime) * breathAmpMoving;
        else
            newPos.y += math.sin(breathAnimTime) * breathAmp;
        torsoRenderer.transform.localPosition = newPos;

        // 양팔 위치
        float armOffset = 0;
        if (isMoving)
            armOffset = math.sin(breathAnimTime + breathArmOffset) * breathAmpMoving;
        else
            armOffset = math.sin(breathAnimTime + breathArmOffset) * breathAmp;

        newPos = leftArmDefaultPos;
        newPos.y += armOffset;
        lArmRenderer.transform.localPosition = newPos;

        newPos = rightArmDefaultPos;
        newPos.y += armOffset;
        rArmRenderer.transform.localPosition = newPos;
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
            if (armDelaySprites.Count <= armAnimTime)
            {
                // 양 팔 위치 지정
                rArmRenderer.transform.localPosition = rightArmPosList[isRightAttack ? 1 : 3];
                lArmRenderer.transform.localPosition = leftArmPosList[isRightAttack ? 3 : 1];

                // 플래그 계산
                isSkillDelay = false;
                armAnimTime = 0;
                torsoAnimTime = 0;

                return;
            }

            // 양 팔 위치 지정
            rArmRenderer.transform.localPosition = rightArmPosList[isRightAttack ? 0 : 2];
            lArmRenderer.transform.localPosition = leftArmPosList[isRightAttack ? 2 : 0];

            // 스프라이트 지정
            torsoRenderer.sprite = isRightAttack ? torsoAttackRight[0] : torsoAttackLeft[0];
            if (isRightAttack)
                rArmRenderer.sprite = armDelaySprites[(int)armAnimTime];
            else
                lArmRenderer.sprite = armDelaySprites[(int)armAnimTime];

            armAnimTime += Time.deltaTime * skillDelaySpeed;
        }
        else
        {
            // 애니 재생 완료인지 먼저 확인
            if (armAttackSprites.Count <= armAnimTime)
            {
                // 스프라이트 복구
                torsoRenderer.sprite = torsoIdle;
                rArmRenderer.sprite = rightArmSprIdle;
                lArmRenderer.sprite = leftArmSprIdle;

                // 플래그 복구
                isAttacking = false;
                isSkillDelay = true;
                armAnimTime = 0;

                return;
            }
            
            if (isRightAttack)
            {
                rArmRenderer.sprite = armAttackSprites[(int)armAnimTime];
                torsoRenderer.sprite = torsoAttackRight[(int)Mathf.Min(torsoAttackLeft.Count - 1, 1 + torsoAnimTime)];
            }
            else
            {
                lArmRenderer.sprite = armAttackSprites[(int)armAnimTime];
                torsoRenderer.sprite = torsoAttackLeft[(int)Mathf.Min(torsoAttackLeft.Count - 1, 1 + torsoAnimTime)];
            }

            armAnimTime += Time.deltaTime * skillAnimSpeed;
            torsoAnimTime += Time.deltaTime * 15;
        }
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
                leftArmSprIdle = (e.Equipped as WeaponData).SprIdle;
                lArmRenderer.sprite = leftArmSprIdle;
                break;

            case EEquipSlotType.RIGHT:
                rightArmSprIdle = (e.Equipped as WeaponData).SprIdle;
                rArmRenderer.sprite = rightArmSprIdle;
                break;
        }
    }

    private void OnPlayerUsedSkill(GEOnPlayerUsedEquiment e)
    {
        // 다리는 공격 애니 없음
        if (e.SlotType == EEquipSlotType.LEG)
            return;

        // 상체 위치 복구
        ResetTorsoPosition();
        rArmRenderer.sprite = rightArmSprIdle;
        lArmRenderer.sprite = leftArmSprIdle;

        // 플래그 계산
        isAttacking = true;
        isSkillDelay = true;
        isRightAttack = e.SlotType == EEquipSlotType.RIGHT;
        armAnimTime = 0;

        // 스프라이트 목록과 속도 할당
        armAttackSprites = (e.Equipment as WeaponData).SprAttack;
        armDelaySprites = (e.Equipment as WeaponData).SprDelay;

        if (armAttackSprites.Count == 0 || armDelaySprites.Count == 0)
        {
            GameManager.Logger.LogError($"{e.Skill.name} 스킬에 플레이어 애니메이션 스프라이트가 올바르게 지정되지 않았습니다.");
            isAttacking = false;
            return;
        }

        skillDelaySpeed = armDelaySprites.Count / e.Skill.PlayerDelay;
        skillAnimSpeed = armAttackSprites.Count / (e.Skill.Duration - e.Skill.PlayerDelay);
    }

    #endregion
}
