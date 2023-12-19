using AlmostEngine;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace RaldiCrackhousePlus.Patches.Character
{

    // keeps track of all the extra stuff van man may ever need
    public class VanManManager : MonoBehaviour
    {
        public bool nextFailIsKidnapping = false;
    }

    public class VanManKidnapping : MonoBehaviour
    {
        public PlayerManager pm;
        MovementModifier moveMod;
        DetentionUi dt;
        public NPC kidnapper;
        float timeBeforeDeath = 15f;
        void Update()
        {
            if ((pm == null) || (kidnapper == null)) return;
            if (moveMod == null)
            {
                moveMod = new MovementModifier(Vector3.zero, 0f);
                pm.plm.am.moveMods.Add(moveMod);
                dt = GameObject.Instantiate<DetentionUi>(RaldiPlugin.detentionUI);
                dt.Initialize(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam, timeBeforeDeath, pm.ec);
                dt.transform.Find("MainText").gameObject.GetComponent<TMP_Text>().text = "Kidnapped!\n\r  seconds remain.";
                pm.plm.transform.position = new Vector3(kidnapper.transform.position.x, pm.plm.transform.position.y, kidnapper.transform.position.z);
            }
            timeBeforeDeath -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
            if ((timeBeforeDeath <= 0f) || (pm.plm.transform.position - kidnapper.transform.position).magnitude > 10f)
            {
                pm.plm.am.moveMods.Remove(moveMod);
                GameObject.Destroy(dt.gameObject);
                GameObject.Destroy(this); //death.
            }
            pm.plm.transform.position = new Vector3(kidnapper.transform.position.x, pm.plm.transform.position.y, kidnapper.transform.position.z);
            moveMod.movementAddend = kidnapper.Navigator.Velocity; //stops audio from being weird
        }
    }


    [HarmonyPatch(typeof(Playtime))]
    [HarmonyPatch("Start")]
    class PlaytimeStartPatch
    {
        static void Prefix(Playtime __instance, ref Animator ___animator, ref SoundObject[] ___audCount, ref SoundObject[] ___audCalls, ref SoundObject ___audLetsPlay, ref SoundObject ___audGo, ref SoundObject ___audCongrats, ref float ___initialCooldown, ref SoundObject ___audSad)
        {
            ___animator.enabled = false;
            SpriteRotator sprR = __instance.gameObject.AddComponent<SpriteRotator>();
            sprR.ReflectionSetVariable("spriteRenderer", __instance.transform.Find("SpriteBase").Find("Sprite").GetComponent<SpriteRenderer>());
            sprR.ReflectionSetVariable("sprites", RaldiPlugin.vanManSprites);
            ___initialCooldown = 25f;
            __instance.gameObject.AddComponent<VanManManager>();
            ___audCount = new SoundObject[]
            {
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_1"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_2"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_3"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_4"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_5"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_6"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_7"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_8"),
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_9"),
            };
            ___audCalls = new SoundObject[]
            {
                RaldiPlugin.assetMan.Get<SoundObject>("vanman_laugh")
            };
            ___audLetsPlay = RaldiPlugin.assetMan.Get<SoundObject>("vanman_letsplay");
            ___audGo = RaldiPlugin.assetMan.Get<SoundObject>("vanman_readygo");
            ___audCongrats = RaldiPlugin.assetMan.Get<SoundObject>("vanman_congrats");
            ___audSad = RaldiPlugin.assetMan.Get<SoundObject>("vanman_thefuck");
        }
    }

    [HarmonyPatch(typeof(Playtime))]
    [HarmonyPatch("EndJumprope")]
    class PlaytimeEndPatch
    {
        static void Prefix(Playtime __instance, ref SoundObject ___audSad)
        {
            VanManManager vmm = __instance.GetComponent<VanManManager>();
            if (vmm.nextFailIsKidnapping)
            {
                vmm.nextFailIsKidnapping = false;
                ___audSad = RaldiPlugin.assetMan.Get<SoundObject>("vanman_kidnap");
                Singleton<CoreGameManager>.Instance.AddPoints(-30, 0, true);
            }
        }

        static void Postfix(ref SoundObject ___audSad)
        {
            ___audSad = RaldiPlugin.assetMan.Get<SoundObject>("vanman_thefuck");
        }

    }

    [HarmonyPatch(typeof(Playtime))]
    [HarmonyPatch("JumpropeHit")]
    class PlaytimeMissPatch
    {
        static bool Prefix(Playtime __instance, Jumprope ___currentJumprope)
        {
            __instance.gameObject.GetComponent<VanManManager>().nextFailIsKidnapping = true;
            ___currentJumprope.End(false);
            VanManKidnapping vmk = __instance.gameObject.AddComponent<VanManKidnapping>();
            vmk.kidnapper = __instance;
            vmk.pm = ___currentJumprope.player;
            return false;
        }
    }
}
