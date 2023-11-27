using HarmonyLib;
using System;
using System.Collections.Generic;
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
                wallTex = RaldiPlugin.cobblestoneWall;
            }
        }
    }

    [HarmonyPatch(typeof(OfficeBuilderStandard))]
    [HarmonyPatch("Build")]
    class OfficeBuilderStandardPatch
    {
        static void Prefix(OfficeBuilderStandard __instance, ref WindowObject ___windowObject)
        {
            
        }
    }
}
