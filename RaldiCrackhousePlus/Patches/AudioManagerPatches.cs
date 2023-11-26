using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(AudioManager))]
    [HarmonyPatch("QueueRandomAudio")]
    class AudioManagerTolerateBullshit
    {
        static bool Prefix(SoundObject[] sounds)
        {
            return sounds != null;
        }
    }
}
