using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CooldownBox : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private Image cooldownImg;
    [SerializeField] private Sprite emptySpr;
    [SerializeField] private Color colorDefault;
    [SerializeField] private Color colorBlink;

    // ===== 쿨다운 =====
    private int cooldownState = 0; // 0 비활성화 1 쿨다운 2 반짝
    private float cdLastTime = -1;
    private float cdMax = 1;
    private readonly float cbBlink = 0.2f; // 쿨다운 종료 후 반짝거릴 시간

    private void Update()
    {
        switch (cooldownState)
        {
            case 1: // 쿨다운 표시
            {
                float elapsedTime = Time.time - cdLastTime;
                float progress = math.min(1, elapsedTime / cdMax); // 0 ~ 1

                cooldownImg.fillAmount = 1 - progress;
                if (cooldownImg.fillAmount <= 0)
                {
                    cooldownState = 2;
                    cdLastTime = Time.time;

                    cooldownImg.color = colorBlink;
                    cooldownImg.fillAmount = 1;
                }
            }
            break;

            case 2: // 쿨다운 완료 반짝임
            {
                float elapsedTime = Time.time - cdLastTime;
                float progress = math.min(1, elapsedTime / cbBlink); // 0 ~ 1

                cooldownImg.color = Color.Lerp(colorBlink, Color.clear, progress);

                if (1 <= progress)
                {
                    cooldownImg.fillAmount = 0;
                    cooldownImg.color = colorDefault;
                    cooldownState = 0;
                }
            }
            break;
        }
    }

    public void SetCooldown(float f)
    {
        cdMax = Mathf.Max(0.01f, f);
        cdLastTime = Time.time;

        cooldownImg.fillAmount = 1;
        cooldownState = 1;
    }

    public void SetIcon(Sprite sprite)
    {
        iconImg.sprite = sprite ?? emptySpr;
    }
}
