using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
   public class PlayerGutsController : MonoBehaviour
    {
        [SerializeField] private Player owner;

        private readonly GutData[] appliedGuts = new GutData[(int)EGutType._LENGTH];

        public PlayerGutRuntime Runtime { get; private set; }

        private void Awake()
        {
            if (!owner)
                owner = GetComponent<Player>();

            Runtime = new PlayerGutRuntime();

            GameManager.Eventbus.Subscribe<GEOnGutChanged>(OnGutChanged);

            Debug.Log("[GUT][PLAY] PlayerGutsController Awake");
            ApplyAllCurrentGuts();
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnGutChanged>(OnGutChanged);
        }

        private void ApplyAllCurrentGuts()
        {
            Debug.Log("[GUT][PLAY] ===== ApplyAllCurrentGuts START =====");

            for (int i = 0; i < (int)EGutType._LENGTH; i++)
            {
                EGutType type = (EGutType)i;
                GutData gut = GameManager.Player.GetGutData(type);

                if (gut == null)
                {
                    Debug.Log($"[GUT][PLAY] {type} = NONE");
                    continue;
                }

                Debug.Log($"[GUT][PLAY] {type} = {gut.ID}");
                ApplyGut(type, gut);
            }

            Runtime.RecalculateAndPublish();

            Debug.Log("[GUT][PLAY] ===== ApplyAllCurrentGuts END =====");
        }

        private void OnGutChanged(GEOnGutChanged e)
        {
            Debug.Log($"[GUT][PLAY] OnGutChanged - type={e.Type}, old={(e.Replaced ? e.Replaced.ID : "null")}, new={(e.Equipped ? e.Equipped.ID : "null")}");

            if (e.Replaced)
                RemoveGut(e.Type, e.Replaced);

            if (e.Equipped)
                ApplyGut(e.Type, e.Equipped);

            Runtime.RecalculateAndPublish();
        }

        private void ApplyGut(EGutType type, GutData gut)
        {
            int idx = (int)type;

            if (appliedGuts[idx] == gut)
            {
                Debug.Log($"[GUT][PLAY] APPLY SKIP - {type} already {gut.ID}");
                return;
            }

            appliedGuts[idx] = gut;

            GutEffectData[] effs = gut.Effects;
            if (effs == null || effs.Length == 0)
            {
                Debug.Log($"[GUT][PLAY] APPLY - {type} {gut.ID}, effects = 0");
                return;
            }

            Debug.Log($"[GUT][PLAY] APPLY - {type} {gut.ID}, effects = {effs.Length}");

            for (int i = 0; i < effs.Length; i++)
            {
                if (effs[i] == null)
                    continue;

                Debug.Log($"[GUT][PLAY]   -> effect[{i}] = {effs[i].name}");
                effs[i].Apply(owner);
            }
        }

        private void RemoveGut(EGutType type, GutData gut)
        {
            int idx = (int)type;
            appliedGuts[idx] = null;

            GutEffectData[] effs = gut.Effects;
            if (effs == null || effs.Length == 0)
                return;

            Debug.Log($"[GUT][PLAY] REMOVE - {type} {gut.ID}, effects = {effs.Length}");

            for (int i = 0; i < effs.Length; i++)
            {
                if (effs[i] == null)
                    continue;

                Debug.Log($"[GUT][PLAY]   -> remove effect[{i}] = {effs[i].name}");
                effs[i].Remove(owner);
            }
        }
    }
}