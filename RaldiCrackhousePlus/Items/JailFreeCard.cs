using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus
{
    public class ITM_JailFreeCard : Item
    {
        public override bool Use(PlayerManager pm)
        {
            DetentionManager[] managers = GameObject.FindObjectsOfType<DetentionManager>();
            if (managers.Length == 0)
            {
                return false;
            }
            managers.Do(m =>
            {
                RoomController rc = pm.ec.offices.Find(x => x.functionObject == m.gameObject);
                if (rc == null)
                {
                    RaldiPlugin.Log.LogWarning("DetentionManager found without room controller??");
                    return;
                }
                rc.doors.Do(x => x.Unlock());
                GameObject.Destroy(m);
            });
            DetentionUi detUi = GameObject.FindObjectsOfType<DetentionUi>().First();
            if (detUi == null)
            {
                RaldiPlugin.Log.LogWarning("DetentionUI not found??");
                return true;
            }
            GameObject.Destroy(detUi.gameObject);
            return true;
        }
    }
}
