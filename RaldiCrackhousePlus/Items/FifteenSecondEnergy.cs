using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus
{
    public class ITM_15SecondEnergy : Item
    {
        public const float speedBoost = 7f;
        public const float speedTime = 15f;
        public override bool Use(PlayerManager pm)
        {
            pm.plm.stamina = pm.plm.staminaMax * 2f;
            pm.plm.walkSpeed += speedBoost;
            pm.plm.runSpeed += speedBoost;
            StartCoroutine(WaitTime(pm));
            return true;
        }

        public IEnumerator WaitTime(PlayerManager pm)
        {
            yield return new WaitForSecondsEnviromentTimescale(pm.ec,speedTime);
            pm.plm.walkSpeed -= speedBoost;
            pm.plm.runSpeed -= speedBoost;
            UnityEngine.Object.Destroy(gameObject);
            yield break;
        }
    }
}
