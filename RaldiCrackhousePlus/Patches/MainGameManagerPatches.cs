using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaldiCrackhousePlus.Patches
{
    /*[HarmonyPatch(typeof(MainGameManager))]
    [HarmonyPatch("BeginPlay")]
    class ChangeMusicPatch
    {
        static void Postfix()
        {
            Singleton<MusicManager>.Instance.SetLoop(false);
            Singleton<MusicManager>.Instance.StopMidi();
            Singleton<MusicManager>.Instance.QueueFile(RaldiPlugin.CrackMusic, true);
        }
    }*/
}
