
namespace LAMENT
{
    public interface IHittable
    {
        public bool OnHit(DamageResponse rsp);
    }

    public struct DamageResponse
    {
        public Entity src;
    }
}