using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(MusicManager))]
    [HarmonyPatch("StopMidi")]
    class StopALLMusic
    {
        static void Postfix(MusicManager __instance)
        {
            __instance.StopFile();
        }
    }

    [HarmonyPatch(typeof(MusicManager))]
    [HarmonyPatch("PlayMidi")]
    class ReplaceMusic
    {
        static bool Prefix(MusicManager __instance, ref string song, bool loop)
        {
            __instance.StopFile();
            if (song == "Elevator")
            {
                song = RaldiPlugin.CrackElevatorMusic;
            }
            if (song == "school")
            {
                __instance.QueueFile(RaldiPlugin.CrackMusic, loop);
                return false;
            }
            return true;
        }
    }
}
