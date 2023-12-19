using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(HappyBaldi))]
    [HarmonyPatch("PlayIntroAudio")]
    class HappyBaldiGreetingPatch
    {
        static void Prefix(ref SoundObject ___audIntro)
        {
            ___audIntro = RaldiPlugin.assetMan.Get<SoundObject>("raldi_greeting");
        }
    }

    [HarmonyPatch(typeof(HappyBaldi))]
    [HarmonyPatch("Activate")]
    class HappyBaldiActivatePatch
    {
        static bool Prefix(HappyBaldi __instance, ref bool ___activated, ref AudioManager ___audMan, ref SpriteRenderer ___sprite, ref EnvironmentController ___ec)
        {
            ___activated = true;
            __instance.StartCoroutine(OppaGangnamStyle(__instance, ___audMan, ___sprite, ___ec));
            return false;
        }
        static IEnumerator OppaGangnamStyle(HappyBaldi __instance, AudioManager audMan, SpriteRenderer spri, EnvironmentController ec)
        {
            yield return null;
            while ((audMan.IsPlaying || Singleton<CoreGameManager>.Instance.Paused) && (Singleton<BaseGameManager>.Instance.FoundNotebooks == 0))
            {
                yield return null;
            }
            audMan.FlushQueue(true);
            Singleton<MusicManager>.Instance.StopFile();
            Singleton<MusicManager>.Instance.StopMidi();
            audMan.PlaySingle(RaldiPlugin.assetMan.Get<SoundObject>("raldi_dance"));
            __instance.GetComponent<Animator>().enabled = false;
            __instance.StartCoroutine(StupidDance(spri));
            while (audMan.IsPlaying || Singleton<CoreGameManager>.Instance.Paused)
            {
                yield return null;
            }
            Singleton<BaseGameManager>.Instance.BeginSpoopMode();
            ec.SpawnNPCs();
            if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Main)
            {
                ec.GetBaldi().transform.position = __instance.transform.position;
            }
            else if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
            {
                ec.GetBaldi().Despawn();
            }
            ec.StartEventTimers();
            spri.enabled = false;
            UnityEngine.Object.Destroy(__instance.gameObject);
            yield break;
        }
        static IEnumerator StupidDance(SpriteRenderer sprit)
        {
            int danceCycle = 0;
            while (true)
            {
                sprit.sprite = RaldiPlugin.RaldiDance[danceCycle % RaldiPlugin.RaldiDance.Count];
                danceCycle++;
                yield return new WaitForSeconds(0.07f);
            }
        }
    }
}
