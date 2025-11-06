using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "ScriptableObjects/Skills/Utility/Dash")]
public class Dash : Skill
{
    public override string Comment => "0번 대시 시작\n1번 대시 종료";

    [SerializeField][Tooltip("앞/뒤 대시 여부")] private bool isFrontDash = true;
    [SerializeField] private float speed = 5f;

    private bool isDashing = false;

    protected override void Perform(Entity owner)
    {
        if (IsTiming(0))
        {
            owner.MoveComponent.SetHSpeed(GetSpeedByDirection(owner.MoveComponent));

            isDashing = true;
            owner.MoveComponent.SetGravityEnabled(false);
        }

        if (IsTiming(1))
        {
            isDashing = false;
            owner.MoveComponent.SetGravityEnabled(true);
        }

        if (isDashing)
            owner.MoveComponent.SetVSpeed(0);
    }

    private float GetSpeedByDirection(MoveComponent mc)
    {
        if (mc.Direction == MoveComponent.EMoveState.RIGHT)
            return isFrontDash ? speed : -speed;
        else
            return isFrontDash ? -speed : speed;
    }
}
