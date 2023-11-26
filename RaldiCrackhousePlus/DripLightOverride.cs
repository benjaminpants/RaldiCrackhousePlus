using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RaldiCrackhousePlus
{
    public class DripLightOverride : MonoBehaviour
    {
        EnvironmentController ec;
        Texture2D myLightMapTex;
        List<LanternSource> sources = new List<LanternSource>();

        private Color _color;
        private Vector3 _position;
        private IntVector2 _updatePos;
        private float _distance;
        public float fade = 0f;
        public void Initialize(EnvironmentController ec)
        {
            ec.lightingOverride = true;
            this.ec = ec;
            Texture2D lightmapTex = Singleton<CoreGameManager>.Instance.lightMapTexture;
            myLightMapTex = new Texture2D(lightmapTex.width, lightmapTex.height, lightmapTex.format, false);
            Graphics.CopyTexture(lightmapTex, myLightMapTex);
        }

        public LanternSource AddSource(Transform trans, float strength, Color color)
        {
            LanternSource lanternSource = new LanternSource();
            lanternSource.transform = trans;
            lanternSource.strength = strength;
            lanternSource.color = color;
            sources.Add(lanternSource);
            return lanternSource;
        }
        private void Update()
        {
            if (this.ec != null)
            {
                for (int i = 0; i < this.ec.levelSize.x; i++)
                {
                    for (int j = 0; j < this.ec.levelSize.z; j++)
                    {
                        this._position.x = (float)i * 10f + 5f;
                        this._position.z = (float)j * 10f + 5f;
                        this._updatePos.x = i;
                        this._updatePos.z = j;
                        this._position.y = 5f;
                        this._color = Color.black;
                        foreach (LanternSource lanternSource in this.sources)
                        {
                            this._distance = Vector3.Distance(lanternSource.transform.position, this._position) / 10f;
                            this._color += lanternSource.color * (1f - Mathf.Clamp(this._distance / lanternSource.strength, 0f, 1f)) * (Color.white - this._color);
                        }
                        Singleton<CoreGameManager>.Instance.UpdateLighting(Color.Lerp(myLightMapTex.GetPixel(_updatePos.x, _updatePos.z), this._color, fade), this._updatePos);
                    }
                }
            }
        }
    }
}
