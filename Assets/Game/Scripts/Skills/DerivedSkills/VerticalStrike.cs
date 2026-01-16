using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "Vertical Strike", menuName = "ScriptableObjects/Skills/Offensive/Vertical Strike")]
    public class VerticalStrike : Skill
    {
        [SerializeField] private float jumpPower = 15f;
        [SerializeField] private bool flipSpriteY = true;

        public override string Comment =>
            "0번 타이밍: 전진 시작\n" +
            "1번 타이밍: 점프 + 판정 켜기\n" +
            "2번 타이밍: 전진 종료\n" +
            "3번 타이밍: 판정 끄기";

        protected override void Perform(Entity owner)
        {
            if (IsTiming(0))
            {
                owner.MoveComponent.SetGravityEnabled(false);
                owner.MoveComponent.SetVSpeed(0);
                owner.MoveComponent.SetMovement(owner.MoveComponent.Direction);
            }

            if (IsTiming(1))
            {
                owner.MoveComponent.SetGravityEnabled(true);
                owner.MoveComponent.SetVSpeed(jumpPower);
                GetEffector(owner).SetEnabled(0, true);
                GetEffector(owner).FlipY(flipSpriteY);
            }

            if (IsTiming(2))
            {
                owner.MoveComponent.SetMovement(MoveComponent.EMoveState.STOP);
            }

            if (IsTiming(3))
            {
                GetEffector(owner).SetEnabled(0, false);
            }
        }
    }
}