using System;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class ParallaxBackground : MonoBehaviour
{
    [Serializable]
    public struct BgrLayer
    {
        public Transform transform;

        public float scrollSpeed;
        public float offsetX;

        public Sprite[] animFrames;
        public float animSpeed;
        [HideInInspector] public float animTimeCurr;
        [HideInInspector] public int frameCurr;

        [HideInInspector] public SpriteRenderer sprL;
        [HideInInspector] public SpriteRenderer sprR;
    }

    [SerializeField]
    private float depth = 10;

    [Header("Target")]
    [SerializeField]
    private Transform followTarget; // 추적할 대상

    [Header("Layers")]
    [SerializeField]
    private float bgrWidth = 62.5f;
    [SerializeField]
    private BgrLayer[] layers;


    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        // 각 레이어의 이미지들 위치 초기화
        for (int i = 0; i < layers.Length; i++)
        {
            Transform left = layers[i].transform.GetChild(0);
            Transform right = layers[i].transform.GetChild(1);

            left.localPosition = new Vector2(0, 0);
            right.localPosition = new Vector2(bgrWidth, 0);

            layers[i].sprL = left.GetComponent<SpriteRenderer>();
            layers[i].sprR = right.GetComponent<SpriteRenderer>();

            layers[i].animTimeCurr = 0;
            layers[i].frameCurr = 0;
        }
    }

    private void LateUpdate()
    {
        UpdateBackground();
    }

    private float GetBgrX(float scrollSpeed, bool isRight, float offset = 0)
    {
        float sign = Mathf.Sign(followTarget.position.x);
        float x = (Mathf.Abs(followTarget.position.x) + offset * sign) * scrollSpeed;

        float xx = ((x + (isRight ? bgrWidth : 0))) % (bgrWidth * 2) - bgrWidth;
        xx *= sign * -1;

        return xx;
    }

    private void UpdateBackground()
    {
        if (!followTarget)
            return;
        
        // 루트 위치 지정
        Vector3 newPosRoot = followTarget.position;
        newPosRoot.z = depth;
        transform.position = newPosRoot;

        // 각 레이어의 이미지들 위치 초기화
        for (int i = 0; i < layers.Length; i++)
        {
            Transform left = layers[i].transform.GetChild(0);
            Transform right = layers[i].transform.GetChild(1);

            // 위치 업데이트
            left.localPosition = new Vector2(GetBgrX(layers[i].scrollSpeed, false, layers[i].offsetX), 0);
            right.localPosition = new Vector2(GetBgrX(layers[i].scrollSpeed, true, layers[i].offsetX), 0);

            // 애니메이션 업데이트
            if (0 < layers[i].animFrames.Length)
            {
                layers[i].animTimeCurr += Time.deltaTime;
                if (layers[i].animSpeed <= layers[i].animTimeCurr)
                {
                    layers[i].animTimeCurr = 0;
                    layers[i].frameCurr = (layers[i].frameCurr + 1) % layers[i].animFrames.Length;
                    layers[i].sprL.sprite = layers[i].animFrames[layers[i].frameCurr];
                    layers[i].sprR.sprite = layers[i].animFrames[layers[i].frameCurr];
                }
            }
        }
    }
}
