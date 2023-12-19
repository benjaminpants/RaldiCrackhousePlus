using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(LevelBuilder))]
    [HarmonyPatch("TextureArea")]
    class LevelBuilderTexturePatch
    {
        static void Prefix(ref RoomController room, ref Texture2D wallTex)
        {
            if (room.category == RoomCategory.Office)
            {
                wallTex = RaldiPlugin.assetMan.Get<Texture2D>("cobblewall");
            }
        }
    }

    [HarmonyPatch(typeof(OfficeBuilderStandard))]
    [HarmonyPatch("Finish")]
    class OfficeBuilderStandardFinalizePatch
    {
        static void Postfix(OfficeBuilderStandard __instance)
        {
            __instance.Room.doors.Do(d =>
            {
                StandardDoor sd = d.GetComponent<StandardDoor>();
                if (sd)
                {
                    sd.overlayShut[0] = RaldiPlugin.JailDoorObject.shut;
                    sd.overlayShut[1] = RaldiPlugin.JailDoorObject.shut;
                    sd.overlayOpen[0] = RaldiPlugin.JailDoorObject.open;
                    sd.overlayOpen[1] = RaldiPlugin.JailDoorObject.open;
                    sd.mask[0] = RaldiPlugin.JailDoorMask;
                    sd.mask[1] = RaldiPlugin.JailDoorMask;
                    sd.UpdateTextures();
                }
            });
        }
    }

    [HarmonyPatch(typeof(OfficeBuilderStandard))]
    [HarmonyPatch("Build")]
    class OfficeBuilderStandardPatch
    {
        static void Prefix(OfficeBuilderStandard __instance, ref WindowObject ___windowObject)
        {
            ___windowObject = RaldiPlugin.JailWindowObject;
        }
    }
}
