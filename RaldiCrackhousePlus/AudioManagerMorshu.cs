using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RaldiCrackhousePlus
{
    public class AudioManagerMorshu : AudioManager
    {
        public Image morshu;
        IEnumerator currentAnim;
        public override void PlayQueue()
        {
            PlayAppropiateMorshuAnimation(soundQueue[0].soundKey);
            base.PlayQueue();
        }

        public override void PlaySingle(SoundObject file)
        {
            base.PlaySingle(file);
            PlayAppropiateMorshuAnimation(file.soundKey);
        }

        void PlayMorshuAnimation(List<Sprite> sprites, int fps, int startFrame = 0, int endFrame = -1)
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }
            currentAnim = PlayAnim(sprites, fps, startFrame, endFrame);
            StartCoroutine(currentAnim);
        }

        void PlayAppropiateMorshuAnimation(string name)
        {
            switch (name)
            {
                case "Vfx_Morshu_Intro":
                    PlayMorshuAnimation(RaldiPlugin.MorshuSprites, 11);
                    break;
                case "Vfx_Morshu_MmmmRicher":
                    PlayMorshuAnimation(RaldiPlugin.MorshuSpritesReject, 11);
                    break;
                case "Vfx_Morshu_Mmmm":
                    PlayMorshuAnimation(RaldiPlugin.MorshuSprites, 11, 15, 18);
                    break;
            }
        }

        IEnumerator PlayAnim(List<Sprite> sprites, int fps, int startFrame = 0, int endFrame = -1)
        {
            if (endFrame == -1) endFrame = sprites.Count;
            for (int i = startFrame; i < endFrame; i++)
            {
                morshu.sprite = sprites[i];
                yield return new WaitForSecondsRealtime((float)(1f /(float)fps));
            }
            yield return new WaitForSecondsRealtime(1f);
            morshu.sprite = RaldiPlugin.MorshuSprites.Last();
            yield break;
        }
    }
}
