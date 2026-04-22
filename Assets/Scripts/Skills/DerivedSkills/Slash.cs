using UnityEngine;


namespace LAMENT
{
    [CreateAssetMenu(fileName = "Slash", menuName = "ScriptableObjects/Skills/Offensive/Slash")]
    public class Slash : Skill
    {
        [SerializeField] private bool movement = true;

        public override string Comment =>
            "0번 타이밍: 전진 시작\n" +
            "1번 타이밍: 판정 켜기 + 전진 종료\n" +
            "2번 타이밍: 판정 끄기";

        protected override void Perform(Entity owner)
        {
            if (IsTiming(0) && movement)
                owner.MoveComponent.SetMovement(owner.MoveComponent.Direction);

            if (IsTiming(1))
            {
                if (movement)
                    owner.MoveComponent.SetMovement(MoveComponent.EDirection.STOP);

                SkillEffector eff = GetEffector(owner);
                if (eff)
                    eff.SetEnabled(0, true);
            }

            if (IsTiming(2))
            {
                // owner.MoveComponent.SetGravityEnabled(true);
                SkillEffector eff = GetEffector(owner);
                if (eff)
                    eff.SetEnabled(0, false);
            }
        }
    }
}