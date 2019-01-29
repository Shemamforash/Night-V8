using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Game.Global
{
    public class AudioClips : MonoBehaviour
    {
        public static AudioClip[] ThunderSounds;
        public static AudioClip[] SMGShots, RifleShots, PistolShots, ShotgunShots;
        public static AudioClip[] SMGCasings, RifleCasings, PistolCasings, ShotgunCasings;
        public static AudioClip[] DryFireClips;
        public static AudioClip[] ArmourBreakClips;
        public static AudioClip[] FootstepClips;
        public static AudioClip[] DayAudio;
        public static AudioClip[] Chimes;
        public static AudioClip Ambient, Night, Campfire, TakeItem;
        public static AudioClip LightRain, MediumRain, HeavyRain;
        public static AudioClip LightWind, MediumWind, HeavyWind;
        public static AudioClip PistolClipIn, PistolClipOut, ShotgunClipIn, ShotgunClipOut, RifleClipIn, RifleClipOut, SMGClipIn, SMGClipOut;
        public static AudioClip BrawlerSlash, BulletHit, ShieldHit, BodyHit;
        public static AudioClip GodsAreDead, AbandonedLands;
        public static AudioClip StandardExplosion, FireExplosion, ShatterExplosion;
        public static AudioClip TabChange, EquipAccessory, EquipArmour, EquipWeapon, Craft, OpenJournal, CloseJournal, Tick;
        public static AudioClip EatWater, EatMeat, EatPlant, EatPotion;
        public static AudioClip ShortHeartBeat, LongHeartBeat;
        public static AudioClip NeedleMove, NeedleHit, NeedleFire;
        public static AudioClip ActiveSkill;
        private static readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();
        private static bool _loaded;

        public void Awake()
        {
            StartCoroutine(LoadAudio());
        }

        private AssetBundle FindAssetBundle(string name)
        {
            return _loadedBundles.FirstOrDefault(b => b.name == name);
        }

        private IEnumerator LoadAssetBundle(string bundleName)
        {
            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleName));
            yield return bundleRequest;
            _loadedBundles.Add(bundleRequest.assetBundle);
        }

        private IEnumerator LoadAllClipsFromBundle(Action<AudioClip[]> setClipAction, string bundleName)
        {
            AssetBundle bundle = FindAssetBundle(bundleName);
            if (bundle == null) yield return StartCoroutine(LoadAssetBundle(bundleName));
            bundle = FindAssetBundle(bundleName);
            Assert.IsNotNull(bundle);
            AssetBundleRequest assetBundleRequest = bundle.LoadAllAssetsAsync<AudioClip>();
            yield return assetBundleRequest;
            AudioClip[] clips = new AudioClip[assetBundleRequest.allAssets.Length];
            for (int i = 0; i < clips.Length; ++i) clips[i] = (AudioClip) assetBundleRequest.allAssets[i];
            setClipAction(clips);
        }

        private IEnumerator LoadClip(Action<AudioClip> setClipAction, string bundleName, string assetName)
        {
            AssetBundle bundle = FindAssetBundle(bundleName);
            if (bundle == null) yield return StartCoroutine(LoadAssetBundle(bundleName));
            bundle = FindAssetBundle(bundleName);
            Assert.IsNotNull(bundle);
            AssetBundleRequest assetBundleRequest = bundle.LoadAssetAsync<AudioClip>(assetName);
            yield return assetBundleRequest;
            setClipAction((AudioClip) assetBundleRequest.asset);
        }

        private IEnumerator LoadAudio()
        {
            if (_loaded) yield break;
            Stopwatch watch = Stopwatch.StartNew();
            Debug.Log("loading shots audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => SMGShots = a, "combat/smg/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => RifleShots = a, "combat/rifle/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => PistolShots = a, "combat/pistol/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ShotgunShots = a, "combat/shotgun/shots"));

            Debug.Log("loading casings audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => SMGCasings = a, "combat/smg/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => RifleCasings = a, "combat/rifle/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => PistolCasings = a, "combat/pistol/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ShotgunCasings = a, "combat/shotgun/casings"));

            Debug.Log("loading misc combat audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => DryFireClips = a, "combat/dryfire"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ArmourBreakClips = a, "combat/armourbreak"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => FootstepClips = a, "combat/footsteps"));
            yield return StartCoroutine(LoadClip(a => StandardExplosion = a, "combat/explosions", "standard explosion"));
            yield return StartCoroutine(LoadClip(a => FireExplosion = a, "combat/explosions", "fire explosion"));
            yield return StartCoroutine(LoadClip(a => ShatterExplosion = a, "combat/explosions", "shatter explosion"));
            yield return StartCoroutine(LoadClip(a => PistolClipIn = a, "combat/reload/clip", "Pistol Clip In"));
            yield return StartCoroutine(LoadClip(a => PistolClipOut = a, "combat/reload/clip", "Pistol Clip Out"));
            yield return StartCoroutine(LoadClip(a => ShotgunClipIn = a, "combat/reload/clip", "Shotgun Clip In"));
            yield return StartCoroutine(LoadClip(a => ShotgunClipOut = a, "combat/reload/clip", "Shotgun Clip Out"));
            yield return StartCoroutine(LoadClip(a => RifleClipIn = a, "combat/reload/clip", "Rifle Clip In"));
            yield return StartCoroutine(LoadClip(a => RifleClipOut = a, "combat/reload/clip", "Rifle Clip Out"));
            yield return StartCoroutine(LoadClip(a => SMGClipIn = a, "combat/reload/clip", "SMG Clip In"));
            yield return StartCoroutine(LoadClip(a => SMGClipOut = a, "combat/reload/clip", "SMG Clip Out"));
            yield return StartCoroutine(LoadClip(a => BrawlerSlash = a, "combat/misc", "Brawler Slash"));
            yield return StartCoroutine(LoadClip(a => ShortHeartBeat = a, "combat/misc", "Short Heart Beat"));
            yield return StartCoroutine(LoadClip(a => LongHeartBeat = a, "combat/misc", "Long Heart Beat"));
            yield return StartCoroutine(LoadClip(a => NeedleMove = a, "combat/misc", "Needle Move"));
            yield return StartCoroutine(LoadClip(a => NeedleHit = a, "combat/misc", "Needle Shatter"));
            yield return StartCoroutine(LoadClip(a => NeedleFire = a, "combat/misc", "Needle Fire"));
            yield return StartCoroutine(LoadClip(a => ActiveSkill = a, "combat/misc", "Active Skill"));
            yield return StartCoroutine(LoadClip(a => BulletHit = a, "combat/misc", "Bullet Hit"));
            yield return StartCoroutine(LoadClip(a => ShieldHit = a, "combat/misc", "Shield Hit"));
            yield return StartCoroutine(LoadClip(a => BodyHit = a, "combat/misc", "Body Hit"));

            Debug.Log("loading weather audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ThunderSounds = a, "thunder"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => DayAudio = a, "daytime"));
            yield return StartCoroutine(LoadClip(a => Night = a, "ambient", "Night"));
            yield return StartCoroutine(LoadClip(a => Ambient = a, "ambient", "Waiting For Time"));
            yield return StartCoroutine(LoadClip(a => LightRain = a, "rain", "Light"));
            yield return StartCoroutine(LoadClip(a => MediumRain = a, "rain", "Medium"));
            yield return StartCoroutine(LoadClip(a => HeavyRain = a, "rain", "Heavy"));
            yield return StartCoroutine(LoadClip(a => LightWind = a, "wind", "Light"));
            yield return StartCoroutine(LoadClip(a => MediumWind = a, "wind", "Medium"));
            yield return StartCoroutine(LoadClip(a => HeavyWind = a, "wind", "Heavy"));

            Debug.Log("loading music");
            yield return StartCoroutine(LoadClip(a => GodsAreDead = a, "music/combat", "The Gods Are Dead"));
            yield return StartCoroutine(LoadClip(a => AbandonedLands = a, "music/combat", "Abandoned Lands"));

            Debug.Log("loading misc audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => Chimes = a, "misc/chimes"));
            yield return StartCoroutine(LoadClip(a => TabChange = a, "misc/inventories", "Tab Change"));
            yield return StartCoroutine(LoadClip(a => TakeItem = a, "misc/inventories", "Item Take"));
            yield return StartCoroutine(LoadClip(a => EquipAccessory = a, "misc/inventories", "Equip Accessory"));
            yield return StartCoroutine(LoadClip(a => EquipArmour = a, "misc/inventories", "Equip Armour"));
            yield return StartCoroutine(LoadClip(a => EquipWeapon = a, "misc/inventories", "Equip Weapon"));
            yield return StartCoroutine(LoadClip(a => Craft = a, "misc/inventories", "Craft"));
            yield return StartCoroutine(LoadClip(a => OpenJournal = a, "misc/inventories", "Open Journal"));
            yield return StartCoroutine(LoadClip(a => CloseJournal = a, "misc/inventories", "Close Journal"));
            yield return StartCoroutine(LoadClip(a => EatMeat = a, "misc/inventories", "Eat Meat"));
            yield return StartCoroutine(LoadClip(a => EatPlant = a, "misc/inventories", "Eat Plant"));
            yield return StartCoroutine(LoadClip(a => EatWater = a, "misc/inventories", "Eat Water"));
            yield return StartCoroutine(LoadClip(a => EatPotion = a, "misc/inventories", "Eat Potion"));
            yield return StartCoroutine(LoadClip(a => Campfire = a, "campfire", "Campfire"));
            yield return StartCoroutine(LoadClip(a => Tick = a, "misc/tick", "Tick"));


            watch.Stop();
            watch.PrintTime("Done loading audio in: ");
            _loaded = true;
        }

        public static bool Loaded()
        {
            return _loaded;
        }
    }
}