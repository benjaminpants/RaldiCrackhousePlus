using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace RaldiCrackhousePlus.Patches.Character
{
    [HarmonyPatch(typeof(Bully))]
    [HarmonyPatch("Start")]
    class BullyStartPatch
    {
        static void Prefix(ref SoundObject ___bored, ref SoundObject ___noItems, ref SoundObject[] ___callouts, ref SoundObject[] ___takeouts)
        {
            ___bored = RaldiPlugin.assetMan.Get<SoundObject>("british_bored");
            ___noItems = RaldiPlugin.assetMan.Get<SoundObject>("british_nopass");
            ___callouts = new SoundObject[]
            {
                RaldiPlugin.assetMan.Get<SoundObject>("british_giveme")
            };
            ___takeouts = new SoundObject[]
            {
                RaldiPlugin.assetMan.Get<SoundObject>("british_thanks")
            };
        }
    }
    [HarmonyPatch(typeof(Bully))]
    [HarmonyPatch("StealItem")]
    class BullyStealPatch
    {

        static FieldInfo slotsToSteal = AccessTools.Field(typeof(Bully), "slotsToSteal");
        static void CheckForEnergy(Bully instance, PlayerManager pm)
        {
            List<int> toSteal = (List<int>)slotsToSteal.GetValue(instance);
            for (int i = 0; i < toSteal.Count; i++)
            {
                if (pm.itm.items[toSteal[i]] == RaldiPlugin.items["15Energy"])
                {
                    slotsToSteal.SetValue(instance, new List<int>()
                    {
                        toSteal[i]
                    });
                    return;
                }
            }
        }

        static MethodInfo checkF = AccessTools.Method(typeof(BullyStealPatch), "CheckForEnergy");
        // TODO: implement this method into endless floors for the Hungry Bully upgrade because this is a much better and less destructive way of implementing it
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // insert calling CheckForEnergy right after the toSteal for loop
        {
            bool didFirstFor = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Blt_S && !didFirstFor) //end of the for loop
                {
                    didFirstFor = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                    yield return new CodeInstruction(OpCodes.Ldarg_1); //pm
                    yield return new CodeInstruction(OpCodes.Call, checkF); //BullyStealPatch.CheckForEnergy
                }
            }
            yield break;
        }
    }
}
