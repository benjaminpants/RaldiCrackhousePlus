using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.AssetManager;
using System.Collections.Generic;
using System;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.Reflection;
using System.Linq;
using UnityEngine.Audio;
using System.Reflection;
using System.IO;

namespace RaldiCrackhousePlus
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInPlugin("mtm101.rulerp.baldiplus.crackhouseplus", "Raldi's Crackhouse Plus", "0.0.0.0")]
    public class RaldiPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        public static RaldiPlugin Instance;
        public static Dictionary<string, SoundObject> RaldiVoicelines = new Dictionary<string, SoundObject>();
        public static Dictionary<string, SoundObject> ChipflokeVoicelines = new Dictionary<string, SoundObject>();
        public static Dictionary<string, SoundObject> MorshuVoicelines = new Dictionary<string, SoundObject>();
        public static Dictionary<string, ItemObject> items = new Dictionary<string, ItemObject>();
        public static SoundObject gunShoot;
        public static WeightedSoundObject[] jumpscareSounds;
        public static List<Sprite> RaldiDance = new List<Sprite>(); //oh god.
        public static List<Sprite> MorshuSprites = new List<Sprite>(); //lamp oil, rope? bombs
        public static List<Sprite> MorshuSpritesReject = new List<Sprite>(); //MMMM Richer!
        //public static List<Sprite> RaldiSlap = new List<Sprite>();
        public static Sprite RaldiDrip;
        public static LoopingSoundObject CrackMusic;
        public static LoopingSoundObject CrackEscapeMusic;
        public static string CrackElevatorMusic;
        private static string posterPath;
        private static string itemPath;
        public static List<WeightedPosterObject> posters = new List<WeightedPosterObject>();

        public static Sprite chipflokeSprite;

        void AddPoster(int weight, params string[] posterNames)
        {
            List<Texture2D> texs = new List<Texture2D>();
            for (int i = 0; i < posterNames.Length; i++)
            {
                texs.Add(AssetManager.TextureFromFile(Path.Combine(posterPath, posterNames[i] + ".png")));
            }
            posters.Add(new WeightedPosterObject()
            {
                weight=weight,
                selection=ObjectCreatorHandlers.CreatePosterObject(texs.ToArray())
            });
        }

        ItemObject CreateItem<T>(string nameInternal, string nameDisplay, string description, string sprite, int price, int genCost) where T : Item
        {
            ItemObject obj = ObjectCreatorHandlers.CreateItemObject(nameDisplay, 
                description, 
                AssetManager.SpriteFromTexture2D(AssetManager.TextureFromFile(Path.Combine(itemPath, sprite + "Small.png")), Vector2.one / 2f, 25f),
                AssetManager.SpriteFromTexture2D(AssetManager.TextureFromFile(Path.Combine(itemPath, sprite + "Big.png")), Vector2.one / 2f, 50f), 
                EnumExtensions.ExtendEnum<Items>(nameInternal),
                price,
                genCost
                );
            obj.item = new GameObject().AddComponent<T>();
            obj.item.name = nameInternal + "Object";
            DontDestroyOnLoad(obj.item);
            items.Add(nameInternal,obj);
            return obj;
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.crackhouseplus");
            Log = this.Logger;
            RaldiVoicelines.Add("greeting", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Raldi", "ral_hi.mp3"), "Vfx_Raldi_Greeting", SoundType.Voice, Color.green));
            RaldiVoicelines.Add("dance", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Music", "mus_style.wav"), "Mus_GangnamStyle", SoundType.Music, Color.white));
            RaldiVoicelines.Add("praise1", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Raldi", "ral_praise1.wav"), "Vfx_Raldi_Praise1", SoundType.Voice, Color.green));
            RaldiVoicelines.Add("seeplayer", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Raldi", "ral_seeplayer.wav"), "Vfx_Raldi_PlayerSpotted", SoundType.Voice, Color.green));
            jumpscareSounds = new WeightedSoundObject[]
            {
                new WeightedSoundObject()
                {
                    selection = ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "fart06.mp3"), "Vfx_Fart", SoundType.Voice, Color.white),
                    weight = 200
                },
                new WeightedSoundObject()
                {
                    selection = ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "fart07.mp3"), "Vfx_Fart", SoundType.Voice, Color.white),
                    weight = 50
                }
            };

            gunShoot = ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "shoot.mp3"), "Vfx_Shoot", SoundType.Effect, Color.white);

            RaldiDrip = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", "Raldi_Drip.png"), Vector2.one / 2f, 32f);

            CrackElevatorMusic = AssetManager.MidiFromFile(Path.Combine(AssetManager.GetModPath(this), "Music", "raldiElevator.mid"), "crack_elevator");

            CrackMusic = ScriptableObject.CreateInstance<LoopingSoundObject>();
            CrackMusic.clips = new AudioClip[]
            {
                AssetManager.AudioClipFromMod(this, "Music", "mus_Crackhouse.mp3")
            };
            CrackMusic.name = "CrackhouseSong";
            CrackMusic.mixer = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().Where(x => x.name == "MIDI").First();

            CrackEscapeMusic = ScriptableObject.CreateInstance<LoopingSoundObject>();
            CrackEscapeMusic.clips = new AudioClip[]
            {
                AssetManager.AudioClipFromMod(this, "Music", "mus_escapeintro.mp3"),
                AssetManager.AudioClipFromMod(this, "Music", "mus_escapeloop.mp3")
            };
            CrackEscapeMusic.name = "CrackhouseSong";
            CrackEscapeMusic.mixer = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().Where(x => x.name == "MIDI").First();

            harmony.PatchAllConditionals();

            for (int i = 0; i < 187; i++)
            {
                RaldiDance.Add(AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", "RaldiDance", String.Format("frame_{0}_delay-0.07s.png",i.ToString("000"))),Vector2.one / 2f, 32f));
            }
            for (int i = 0; i < 88; i++)
            {
                MorshuSprites.Add(AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", "Morshu", "Intro", String.Format("Frame {0}.png", (i + 1).ToString())), Vector2.one / 2f, 1f));
            }
            for (int i = 0; i < 70; i++)
            {
                MorshuSpritesReject.Add(AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", "Morshu", "Reject", String.Format("Frame {0}.png", (i + 1).ToString())), Vector2.one / 2f, 1f));
            }
            MorshuVoicelines.Add("intro", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "morshustore.wav"), "Vfx_Morshu_Intro", SoundType.Voice, new Color(220f/255f,67/255f,16/255f)));
            MorshuVoicelines.Add("reject", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "morshureject.wav"), "Vfx_Morshu_MmmmRicher", SoundType.Voice, new Color(220f / 255f, 67 / 255f, 16 / 255f)));
            MorshuVoicelines.Add("mmm", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "morshummm.wav"), "Vfx_Morshu_Mmmm", SoundType.Voice, new Color(220f / 255f, 67 / 255f, 16 / 255f)));

            ChipflokeVoicelines.Add("10", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_10.wav"), "Vfx_PRI_10", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("15", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_15.wav"), "Vfx_PRI_15", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("30", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_30.wav"), "Vfx_PRI_30", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("45", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_45.wav"), "Vfx_PRI_45", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("60", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_60.wav"), "Vfx_PRI_60", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("99", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_99.wav"), "Vfx_PRI_99", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("running", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_running.wav"), "Vfx_PRI_NoRunning", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("drinking", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_drinking.wav"), "Vfx_PRI_NoDrinking", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("bullying", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_bullying.wav"), "Vfx_PRI_NoBullying", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("lockers", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_lockers.wav"), "Vfx_PRI_NoLockers", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("escaping", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_escape.wav"), "Vfx_PRI_NoEscaping", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("faculty", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_faculty.wav"), "Vfx_PRI_NoFaculty", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("afterhours", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_afterhours.wav"), "Vfx_PRI_NoAfterHours", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("whistle", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_whistle.wav"), "Vfx_PRI_Whistle", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("jailtime", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_jailtime.wav"), "Vfx_PRI_Detention", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("coming", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_coming.wav"), "Vfx_PRI_Coming", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("scold1", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_knowbetter.wav"), "Vfx_PRI_Scold1", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("scold2", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_scold02.wav"), "Vfx_PRI_Scold2", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("scold3", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_scold03.wav"), "Vfx_PRI_Scold3", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            ChipflokeVoicelines.Add("scold4", ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "Sounds", "Chipfloke", "chip_scold03.wav"), "Vfx_PRI_Scold4", SoundType.Voice, new Color(133f / 255f, 79f / 255f, 63f / 255f)));
            /*for (int i = 0; i < 5; i++)
            {
                RaldiSlap.Add(AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", String.Format("Raldi_Slap_{0}.png", i.ToString("0"))), Vector2.one / 2f, 32f));
            }*/
            Log.LogDebug("Loading all Raldi frames... unfortunately...");

            posterPath = Path.Combine(AssetManager.GetModPath(this), "Textures", "Posters");
            AddPoster(100, "Poster_ChipRules01");
            AddPoster(60, "Poster_ChipRules02");
            AddPoster(60, "Poster_Confusing");
            AddPoster(80, "Poster_Cookie");
            AddPoster(90, "Poster_Pee");
            AddPoster(50, "Poster_Pipe");
            AddPoster(60, "Comic_01_00", "Comic_01_01");
            AddPoster(60, "Comic_02_00", "Comic_02_01");
            AddPoster(60, "Comic_03_00", "Comic_03_01");
            AddPoster(20, "AsianSticker01", "AsianSticker02");
            AddPoster(70, "Poster_GustavR01");
            AddPoster(70, "Poster_Ronio");
            AddPoster(80, "Poster_FreeCow");
            AddPoster(60, "Poster_Crack");
            AddPoster(2, "Poster_EasterEgg");
            AddPoster(80, "Poster_Club");
            AddPoster(80, "Poster_Player");
            AddPoster(75, "Poster_Peter");
            AddPoster(10, "Poster_Beast00");
            AddPoster(10, "Poster_Beast01");
            AddPoster(10, "Poster_Beast02");
            AddPoster(10, "Poster_Beast03");
            AddPoster(10, "Poster_Beast04");
            AddPoster(10, "Poster_Beast05");
            AddPoster(10, "Poster_Beast06");
            AddPoster(10, "Poster_Beast07");
            AddPoster(60, "Poster_AddMe");
            itemPath = Path.Combine(AssetManager.GetModPath(this), "Sprites", "Items");
            CreateItem<ITM_15SecondEnergy>("15Energy","Itm_15Energy", "Desc_15Energy", "Energy", 65, 45);
            CreateItem<ITM_JailFreeCard>("JailFree", "Itm_JailFree", "Desc_JailFree", "Card", 80, 55);

            chipflokeSprite = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Sprites", "Chipfloke.png"), Vector2.one / 2f, 65f);

            GeneratorManagement.Register(this, GenerationModType.Addend, (string floorName, int floorId, LevelObject obj) =>
            {
                obj.maxClassRooms += 1;
                obj.minClassRooms += 1;
                int changedBaldis = 0;
                obj.potentialBaldis.Do(x =>
                {
                    Raldi rald = x.selection.gameObject.GetComponent<Raldi>();
                    if (rald != null)
                    {
                        GameObject.DestroyImmediate(x.selection);
                        x.selection = rald;
                        changedBaldis++;
                    }
                });

                obj.items = obj.items.AddRangeToArray(new WeightedItemObject[]
                {
                    new WeightedItemObject()
                    {
                        weight = 90 - (floorId * 10),
                        selection = items["15Energy"]
                    },
                    new WeightedItemObject()
                    {
                        weight = 30 + (floorId * 10),
                        selection = items["JailFree"]
                    }
                });

                obj.shopItems = obj.shopItems.AddRangeToArray(new WeightedItemObject[]
                {
                    new WeightedItemObject()
                    {
                        weight = 80 + (floorId * 10),
                        selection = items["15Energy"]
                    },
                    new WeightedItemObject()
                    {
                        weight = 40 + (floorId * 15),
                        selection = items["JailFree"]
                    }
                });

                obj.posters = obj.posters.AddRangeToArray(RaldiPlugin.posters.ToArray());
                Log.LogDebug("Succesfully cleaned up and destroyed " + changedBaldis + " Baldis!");
                obj.MarkAsNeverUnload();
            });
            MTM101BaldiDevAPI.SavesEnabled = false;
        }

        internal static void TransformIntoRaldi(Baldi b)
        {
            Raldi r = b.gameObject.AddComponent<Raldi>();
            r.ReflectionSetVariable("slapCurve",b.ReflectionGetVariable("slapCurve"));
            r.ReflectionSetVariable("speedCurve", b.ReflectionGetVariable("speedCurve"));
            r.ReflectionSetVariable("slap", b.ReflectionGetVariable("slap"));
            r.ReflectionSetVariable("speedMultiplier", b.ReflectionGetVariable("speedMultiplier"));
            r.loseSounds = RaldiPlugin.jumpscareSounds;
            r.ReflectionSetVariable("audAppleThanks", RaldiPlugin.jumpscareSounds.First().selection); //lol
            r.ReflectionSetVariable("correctSounds", new WeightedSoundObject[]
            {
                new WeightedSoundObject()
                {
                    selection=RaldiPlugin.RaldiVoicelines["praise1"],
                    weight=100,
                }
            });
            r.ReflectionSetVariable("baseAnger", b.ReflectionGetVariable("baseAnger"));
            r.ReflectionSetVariable("baseSpeed", b.ReflectionGetVariable("baseSpeed"));
            r.ReflectionSetVariable("audMan", b.ReflectionGetVariable("audMan"));
            // NPC STUFF
            r.ReflectionSetVariable("character", Character.Baldi);
            r.ReflectionSetVariable("navigator", b.ReflectionGetVariable("navigator"));
            r.ReflectionSetVariable("poster", b.ReflectionGetVariable("poster"));
            r.ReflectionSetVariable("looker", b.ReflectionGetVariable("looker"));
            r.ReflectionSetVariable("spriteBase", b.ReflectionGetVariable("spriteBase"));
            r.ReflectionSetVariable("spriteRenderer", b.ReflectionGetVariable("spriteRenderer"));
            r.ReflectionSetVariable("spawnableRooms", b.ReflectionGetVariable("spawnableRooms"));
            r.ReflectionSetVariable("ignorePlayerOnSpawn", b.ReflectionGetVariable("ignorePlayerOnSpawn"));
            r.ReflectionSetVariable("ignoreBelts", b.ReflectionGetVariable("ignoreBelts"));
            r.ReflectionSetVariable("animator", b.ReflectionGetVariable("animator"));
            r.ReflectionSetVariable("aggroed", b.ReflectionGetVariable("aggroed")); //not sure if this one is necessary
            // NAVIGATOR/LOOKER STUFF
            r.ReflectionGetVariable("navigator").ReflectionSetVariable("npc", r);
            r.ReflectionGetVariable("looker").ReflectionSetVariable("npc", r);
            RaldiPlugin.Log.LogDebug("Transformed: " + b.gameObject.name + " into Raldi succesfully!");
            // unfortunately we have to keep baldi alive just for a bit longer...
            // GameObject.DestroyImmediate(b);
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class NameAwakePatch
    {
        static void Prefix()
        {
            Baldi[] Baldis = Resources.FindObjectsOfTypeAll<Baldi>().Where(x => x.GetType() == typeof(Baldi)).ToArray();
            for (int i = 0; i < Baldis.Length; i++)
            {
                RaldiPlugin.TransformIntoRaldi(Baldis[i]);
            }
            FieldInfo speed = AccessTools.Field(typeof(GottaSweep),"speed");
            FieldInfo audIntro = AccessTools.Field(typeof(GottaSweep), "audIntro");
            FieldInfo audSweep = AccessTools.Field(typeof(GottaSweep), "audSweep");
            SoundObject gwIntro = ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(RaldiPlugin.Instance, "Sounds", "gw_weepingtime.mp3"), "Vfx_GottaWeep_Intro", SoundType.Voice, Color.gray);
            SoundObject gwSweep = ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(RaldiPlugin.Instance, "Sounds", "gw_intro.mp3"), "Vfx_GottaWeep_Sweep", SoundType.Voice, Color.gray);
            Resources.FindObjectsOfTypeAll<GottaSweep>().Do(x =>
            {
                audIntro.SetValue(x, gwIntro);
                audSweep.SetValue(x, gwSweep);
                speed.SetValue(x,((float)speed.GetValue(x)) / 5f);
            });
            Resources.FindObjectsOfTypeAll<Beans>().Do(x =>
            {
                x.ReflectionSetVariable("audSpit", RaldiPlugin.gunShoot);
                //x.gum.ReflectionSetVariable("speed", ((float)x.gum.ReflectionGetVariable("speed")) * 2f);
            });
            AssetManager.ReplaceAllTexturesFromFolder(Path.Combine(AssetManager.GetModPath(RaldiPlugin.Instance), "TextureReplacements"));
            //Graphics.CopyTexture(AssetManager.TextureFromMod(RaldiPlugin.Instance, "test.png"), Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.name == "Tubes (3)").First());
        }
    }
}
