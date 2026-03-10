
namespace LAMENT
{
    public interface IHittable
    {
        public bool OnHit(DamageResponse rsp);
    }

    public struct DamageResponse
    {
        public Entity src; // 공격자
        public int amount; // 대미지 총량
    }
}