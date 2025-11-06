using UnityEngine;


[CreateAssetMenu(fileName = "Slash", menuName = "ScriptableObjects/Skills/Offensive/Slash")]
public class Slash : Skill
{
    public override string Comment =>
        "0번 타이밍: 전진 시작\n" +
        "1번 타이밍: 판정 켜기 + 전진 종료\n" +
        "2번 타이밍: 판정 끄기";

    protected override void Perform(Entity owner)
    {
        if (IsTiming(0))
        {
            owner.MoveComponent.SetMovement(owner.MoveComponent.Direction);
        }

        if (IsTiming(1))
        {
            owner.MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
            GetEffector(owner).SetEnabled(0, true);
        }

        if (IsTiming(2))
        {
            GetEffector(owner).SetEnabled(0, false);
        }
    }
}
