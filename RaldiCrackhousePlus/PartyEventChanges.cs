using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus.Patches
{
    [HarmonyPatch(typeof(PartyEvent))]
    [HarmonyPatch("Begin")]
    class PartyEventBeginPatch
    {
        static void Postfix(RoomController ___office)
        {
            ___office.gameObject.AddComponent<PartyEventDiscoLights>();
        }
    }

    [HarmonyPatch(typeof(PartyEvent))]
    [HarmonyPatch("End")]
    class PartyEventEndPatch
    {
        static void Postfix(RoomController ___office)
        {
            ___office.gameObject.GetComponent<PartyEventDiscoLights>().Destroy();
        }
    }
}

namespace RaldiCrackhousePlus
{
    public class PartyEventDiscoLights : MonoBehaviour
    {
        RoomController roomController;

        IEnumerator numerator;

        bool shouldContinueLights = true;

        void Start()
        {
            roomController = gameObject.GetComponent<RoomController>();
            numerator = DiscoLights();
            StartCoroutine(numerator);
        }

        public void Destroy()
        {
            shouldContinueLights = false;
        }

        IEnumerator DiscoLights()
        {
            // figure out the lights that are in the room
            EnvironmentController ec = roomController.ec;
            List<TileController> tlist = roomController.GetNewTileList();
            List<TileController> lights = new List<TileController>();
            List<Color> lightColors = new List<Color>();
            for (int i = 0; i < ec.lights.Count; i++)
            {
                TileController tile = ec.lights[i];
                if (tlist.Contains(tile) && tile.hasLight)
                {
                    lights.Add(tile);
                    lightColors.Add(tile.lightColor);
                }
            }
            while (shouldContinueLights)
            {
                Color lightColor = UnityEngine.Random.ColorHSV(0f,1f,1f,1f,1f,1f);
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].lightColor = lightColor;
                    ec.SetLight(lights[i].lightOn, lights[i]);
                }
                yield return new WaitForSecondsEnviromentTimescale(ec, 0.3636f);
            }
            //reset everything and destroy myself
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].lightColor = lightColors[i];
                ec.SetLight(lights[i].lightOn, lights[i]);
            }
            GameObject.Destroy(this);
        }
    }
}
