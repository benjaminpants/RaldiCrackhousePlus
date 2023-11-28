using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(Beans))]
    [HarmonyPatch("Start")]
    class BeansBulletPatch
    {
        static FieldInfo gumSpeed = AccessTools.Field(typeof(Gum), "speed");
        static void Postfix(Beans __instance)
        {
            gumSpeed.SetValue(__instance.gum, (float)gumSpeed.GetValue(__instance.gum) * 2f);
        }
    }
}
