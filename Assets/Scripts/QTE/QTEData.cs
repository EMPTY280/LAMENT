using UnityEngine;

namespace LAMENT
{
    public enum EQTETriggerType
    {
        ComboFinisher,
        AfterDash
    }

    public enum EQTESequenceMode
    {
        Fixed,
        Random
    }

    public enum EQTEDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [CreateAssetMenu(fileName = "QTEData", menuName = "ScriptableObjects/QTE/QTE Data")]
    public sealed class QTEData : ScriptableObject
    {
        [Header("기본")]
        [SerializeField] private string qteId;
        [SerializeField] private EQTETriggerType triggerType = EQTETriggerType.ComboFinisher;
        [SerializeField] private EQTESequenceMode sequenceMode = EQTESequenceMode.Fixed;

        [Header("입력 시퀀스")]
        [SerializeField] private EQTEDirection[] fixedSequence;
        [SerializeField] private int randomSequenceLength = 3;

        [Header("시간")]
        [SerializeField] private float timeLimit = 1.0f;
        [SerializeField] private float triggerWindow = 0.2f;

        [Header("연출")]
        [SerializeField, Range(0.01f, 1f)] private float slowScale = 0.2f;

        [Header("성공 보상")]
        [SerializeField] private float successDamageMultiplier = 1.5f;
        [SerializeField] private bool preventBurstConsumeOnSuccess = false;

        [Header("실패 정책")]
        [SerializeField] private bool failOnWrongInput = false;

        public string QteId => qteId;
        public EQTETriggerType TriggerType => triggerType;
        public EQTESequenceMode SequenceMode => sequenceMode;
        public EQTEDirection[] FixedSequence => fixedSequence;
        public int RandomSequenceLength => randomSequenceLength;
        public float TimeLimit => timeLimit;
        public float TriggerWindow => triggerWindow;
        public float SlowScale => slowScale;
        public float SuccessDamageMultiplier => successDamageMultiplier;
        public bool PreventBurstConsumeOnSuccess => preventBurstConsumeOnSuccess;
        public bool FailOnWrongInput => failOnWrongInput;
    }
}