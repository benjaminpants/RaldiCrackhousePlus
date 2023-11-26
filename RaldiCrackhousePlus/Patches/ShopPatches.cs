using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("Start")]
    class ShopStartPatch
    {
        static void Prefix(StoreScreen __instance, ref AudioManager ___audMan, ref SoundObject ___audJonIntro, ref SoundObject[] ___audIntroP2, ref SoundObject[] ___audUnafforable, ref SoundObject[] ___audMapFilled, ref SoundObject[] ___audBuy, ref SoundObject[] ___audLeaveSad, ref SoundObject[] ___audLeaveHappy)
        {
            ___audBuy = new SoundObject[]
            {
                RaldiPlugin.MorshuVoicelines["mmm"]
            };
            ___audMapFilled = ___audBuy;
            ___audLeaveHappy = null;
            ___audLeaveSad = null;

            ___audJonIntro = RaldiPlugin.MorshuVoicelines["intro"];
            ___audIntroP2 = null;
            ___audUnafforable = new SoundObject[]
            {
                RaldiPlugin.MorshuVoicelines["reject"]
            };

            Transform canvas = __instance.transform.Find("Canvas");
            canvas.Find("JohnnyMouth").gameObject.SetActive(false);
            Image morshu = canvas.Find("JohnnyBase").GetComponent<Image>();
            morshu.rectTransform.sizeDelta = new Vector2(230f,150f);
            bool ignoreList = ___audMan.ignoreListenerPause;
            AudioSource src = ___audMan.audioDevice;
            GameObject audGam = ___audMan.gameObject;
            GameObject.Destroy(___audMan);
            AudioManagerMorshu am = audGam.AddComponent<AudioManagerMorshu>();
            am.morshu = morshu;
            am.audioDevice = src;
            am.ignoreListenerPause = ignoreList;
            am.positional = false;
            am.pitchModifier = 1f;
            am.useUnscaledPitch = true;
            ___audMan = am;
        }
    }
}
