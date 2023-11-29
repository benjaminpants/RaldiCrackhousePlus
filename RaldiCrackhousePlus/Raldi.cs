using AlmostEngine;
using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus
{
    public class Raldi : Baldi
    {
        private MethodInfo OriginalUpdate = AccessTools.Method(typeof(Baldi),"Update");
        private MethodInfo OriginalStart = AccessTools.Method(typeof(Baldi), "Start");
        private FieldInfo animatorReflect = AccessTools.Field(typeof(Baldi),"animator");
        private FieldInfo slapCurveReflect = AccessTools.Field(typeof(Baldi), "slapCurve");
        private AnimationCurve mySlapCurve => (AnimationCurve)slapCurveReflect.GetValue(this);
        //private FieldInfo otherSourceQueuedMM = AccessTools.Field(typeof(MusicManager), "otherSourceQueued");
        protected bool inDripMode = false;
        private bool preparingForDrip = false;
        protected int countWhenDripMode = 0; //notebook count when we got drip mode
        protected DripLightOverride DLO;
        public override void PlayerInSight(PlayerManager player)
        {
            bool oldAggroed = aggroed;
            base.PlayerInSight(player);
            if (aggroed && !oldAggroed)
            {
                AudMan.PlaySingle(RaldiPlugin.RaldiVoicelines["seeplayer"]);
            }
        }

        public void Start()
        {
            //fake inheritence
            OriginalStart.Invoke(this, null);
        }

        public void StartDripMode()
        {
            StopAllCoroutines();
            inDripMode = true;
            ((Animator)animatorReflect.GetValue(this)).enabled = false; //i hope you DIE in a FIRE!
            spriteRenderer[0].sprite = RaldiPlugin.RaldiDrip;
            countWhenDripMode = Singleton<BaseGameManager>.Instance.FoundNotebooks;
            StopAllCoroutines();
        }

        public void SetDelayRandom()
        {
            delay = mySlapCurve.Evaluate(anger + extraAnger);
            delay /= UnityEngine.Random.Range(1f,2f);
        }

        public void ForceDripModePause()
        {
            StartCoroutine(DripModePause());
        }

        protected override void Slapped()
        {
            base.Slapped();
            SetDelayRandom();
        }

        public IEnumerator DripModePause()
        {
            preparingForDrip = true;
            Singleton<MusicManager>.Instance.QueueFile(RaldiPlugin.CrackEscapeMusic, true);
            DLO = this.gameObject.AddComponent<DripLightOverride>();
            DLO.Initialize(ec);
            LanternSource ls = DLO.AddSource(this.transform, 3f, Color.green);
            float totalTime = 10.178f;
            float trans = 0f;
            float timeRemaining = totalTime;
            while (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                trans += (Time.deltaTime / totalTime) / 2f;
                DLO.fade = trans;
                ls.strength = (trans * 2f) * 3f;
                yield return null;
            }
            trans = 0.5f;
            ls.strength = (trans * 2f) * 3f;
            StartDripMode();
            yield break;
        }

        public override float DistanceCheck(float val)
        {
            if (inDripMode) return val;
            return base.DistanceCheck(val);
        }

        public void Update()
        {
            //fake inheritence
            OriginalUpdate.Invoke(this, null);
            if (inDripMode)
            {
                slapDistance = nextSlapDistance;
                nextSlapDistance = 0f;
                navigator.SetSpeed(slapDistance * 95f);
                navigator.maxSpeed = navigator.speed;
                return;
            }
            if (preparingForDrip) return;
            if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free) return;
            if (Singleton<BaseGameManager>.Instance.FoundNotebooks >= Singleton<BaseGameManager>.Instance.NotebookTotal)
            {
                StartCoroutine(DripModePause());
            }
        }
    }
}
