using UnityEngine;
using UnityEngine.UI;

public class CooldownBox : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private Image cooldownImg;
    [SerializeField] private Sprite emptySpr;

    private float cooldownMax = 1;
    private float cooldownCurr = 0;

    private void Update()
    {
        if (cooldownCurr <= 0)
            return;

        cooldownCurr -= Mathf.Max(0, Time.deltaTime);

        cooldownImg.fillAmount = cooldownCurr / cooldownMax;
    }

    public void SetCooldown(float f)
    {
        cooldownMax = Mathf.Max(0.01f, f);
        cooldownCurr = f;

        cooldownImg.fillAmount = 1;
    }

    public void SetIcon(Sprite sprite)
    {
        iconImg.sprite = sprite ?? emptySpr;
    }
}
