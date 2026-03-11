
namespace LAMENT
{
    public interface IHittable
    {
        public bool OnHitTaken(DamageResponse rsp);
    }

    public struct DamageResponse
    {
        public Entity src; // 공격자
        public float amount; // 대미지 총량
    }
}