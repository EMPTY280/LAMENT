namespace LAMENT
{
    /// <summary>
    /// 현재 장착된 장기 목록을 기준으로 플레이어의 장기 효과를 다시 계산한다.
    /// 런타임 세션 유지용 재계산 전용 클래스.
    /// </summary>
    public sealed class PlayerGutRuntime
    {
        private readonly Player owner;

        public PlayerGutRuntime(Player owner)
        {
            this.owner = owner;
        }

        public void Rebuild()
        {
            if (owner == null)
                return;

            owner.ResetGutRuntimeAttributes();

            for (int i = 0; i < (int)EGutType._LENGTH; i++)
            {
                GutData data = GameManager.Player.GetGutData((EGutType)i);
                if (!data)
                    continue;

                if (data.Effects == null)
                    continue;

                for (int j = 0; j < data.Effects.Length; j++)
                {
                    GutEffectData effect = data.Effects[j];
                    if (effect == null)
                        continue;

                    effect.Apply(owner);
                }
            }

            owner.OnGutRuntimeRebuilt();
        }
    }
}