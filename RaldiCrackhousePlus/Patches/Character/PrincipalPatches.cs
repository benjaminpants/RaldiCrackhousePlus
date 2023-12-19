using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaldiCrackhousePlus.Patches.Character
{
    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("SendToDetention")]
    public class PrincipalDetentionPatch
    {
        static void Prefix(ref float ___detentionInit, ref float ___detentionInc, ref int ___detentionLevel, ref SoundObject[] ___audTimes, PlayerManager ___targetedPlayer)
        {
            ___detentionLevel = 0;
            ___detentionInc = 0;
            switch (___targetedPlayer.ruleBreak)
            {
                default:
                    RaldiPlugin.Log.LogWarning("Unknown rulebreak: " + ___targetedPlayer.ruleBreak + "!\nDefaulting to 15...");
                    ___detentionInit = 15;
                    break;
                case "Running":
                    ___detentionInit = 10;
                    break;
                case "Drinking":
                    ___detentionInit = 15;
                    break;
                case "Faculty":
                    ___detentionInit = 20;
                    break;
                case "Bullying":
                    ___detentionInit = 10;
                    break;
                case "AfterHours":
                    ___detentionInit = 99;
                    break;
                case "Lockers":
                    ___detentionInit = 30;
                    break;
                case "Escaping":
                    ___detentionInit = 45;
                    break;
            }
            string keyToTry = ((int)___detentionInit).ToString();
            if (RaldiPlugin.assetMan.ContainsKey("chipfloke_" + keyToTry))
            {
                ___audTimes = new SoundObject[]
                {
                    RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_" + keyToTry),
                    RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_" + keyToTry),
                };
            }
            else
            {
                ___audTimes = new SoundObject[]
                {
                    RaldiPlugin.assetMan.Get<SoundObject>("morshu_mmm"),
                    RaldiPlugin.assetMan.Get<SoundObject>("morshu_mmm"),
                };
            }
        }
    }

    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("Start")]
    public class PrincipalStartPatches
    {
        static void Postfix(Principal __instance, ref SoundObject ___audComing, ref SoundObject ___audWhistle, ref SoundObject ___audDetention, ref SoundObject[] ___audScolds)
        {
            ___audComing = RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_coming");
            ___audWhistle = RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_whistle");
            ___audDetention = RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_jailtime");
            ___audScolds = new SoundObject[]
            {
                RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_scold1"),
                RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_scold2"),
                RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_scold3"),
                RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_scold4"),
            };
            __instance.spriteRenderer[0].sprite = RaldiPlugin.chipflokeSprite;
        }
    }

    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("Scold")]
    public class PrincipalScoldPatch
    {
        static bool Prefix(Principal __instance, AudioManager ___audMan, string brokenRule)
        {
            if (!RaldiPlugin.assetMan.ContainsKey("chipfloke_" + brokenRule.ToLower()))
            {
                return true;
            }
            ___audMan.FlushQueue(true);
            ___audMan.QueueAudio(RaldiPlugin.assetMan.Get<SoundObject>("chipfloke_" + brokenRule.ToLower()));
            return false;
        }
    }
}
