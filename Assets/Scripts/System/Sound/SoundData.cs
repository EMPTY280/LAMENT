using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    [CreateAssetMenu(fileName = "New Sound Data", menuName = "ScriptableObjects/Sound/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [Header("기본")]
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField] private ESoundCategory category = ESoundCategory.SFX;

        [Header("재생")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        [SerializeField] private bool loop = false;

        [Header("중복 재생")]
        [SerializeField] private ESoundOverlapPolicy overlapPolicy = ESoundOverlapPolicy.AllowOverlap;
        [SerializeField] private int overlapLimit = 3;
        [SerializeField] private float cooldown = 0.03f;

        public string ID => id;
        public AudioClip Clip => clip;
        public ESoundCategory Category => category;
        public float Volume => volume;
        public bool Loop => loop;
        public ESoundOverlapPolicy OverlapPolicy => overlapPolicy;
        public int OverlapLimit => overlapLimit;
        public float Cooldown => cooldown;
    }

}
