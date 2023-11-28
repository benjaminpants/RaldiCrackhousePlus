using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(ObjectPlacer))]
    [HarmonyPatch("Build")]
    class ObjectBuilderPostPatch
    {
        static FieldInfo itemFo = AccessTools.Field(typeof(SodaMachine), "item");
        static FieldInfo requiredItemFo = AccessTools.Field(typeof(SodaMachine), "requiredItem");
        static FieldInfo outOfStockMat = AccessTools.Field(typeof(SodaMachine), "outOfStockMat");
        static FieldInfo meshRenderer = AccessTools.Field(typeof(SodaMachine), "meshRenderer");
        static void Postfix(ObjectPlacer __instance, System.Random cRng, GameObject ___prefab)
        {
            if (!___prefab.GetComponent<SodaMachine>()) return;
            if (((ItemObject)itemFo.GetValue(___prefab.GetComponent<SodaMachine>())).itemType != Items.ZestyBar) return;
            List<SodaMachine> machines = new List<SodaMachine>();
            __instance.ObjectsPlaced.Do(x =>
            {
                machines.Add(x.GetComponent<SodaMachine>());
            });
            machines.Do(x =>
            {
                if (cRng.Next(1, 3) == 1)
                {
                    x.name = "15EnergyMachine";
                    Material[] sodMat = RaldiPlugin.SodaMachineMaterials["15Energy"];
                    itemFo.SetValue(x, RaldiPlugin.items["15Energy"]);
                    MeshRenderer mr = (MeshRenderer)meshRenderer.GetValue(x);
                    Material[] thisMat = mr.sharedMaterials;
                    thisMat[1] = sodMat[0];
                    mr.sharedMaterials = thisMat;
                    outOfStockMat.SetValue(x, sodMat[1]);
                    requiredItemFo.SetValue(x, RaldiPlugin.items["HalfDollar"]);
                }
            });
        }
    }
}
