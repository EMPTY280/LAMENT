namespace LAMENT
{
    public enum EOffensiveSkillType
    {
        Slash,
        Vertical
    }

    public abstract class OffensiveSkill : Skill
    {
        public abstract EOffensiveSkillType OffensiveType { get; }
    }
}
