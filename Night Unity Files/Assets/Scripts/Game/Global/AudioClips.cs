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
        public static AudioClip[] LMGShots, SMGShots, RifleShots, PistolShots, ShotgunShots;
        public static AudioClip[] LMGCasings, SMGCasings, RifleCasings, PistolCasings, ShotgunCasings;
        public static AudioClip[] DryFireClips;
        public static AudioClip[] ArmourBreakClips;
        public static AudioClip[] FootstepClips;
        public static AudioClip[] ExplosionClips;
        public static AudioClip[] LightRainClips, MediumRainClips, HeavyRainClips;
        public static AudioClip[] LightWindClips, MediumWindClips, HeavyWindClips;
        public static AudioClip[] DayAudio, NightAudio;
        public static AudioClip[] Ambient;
        public static AudioClip ButtonSelectClip;
        public static AudioClip SimmavA, SimmavB, SimmavC;
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
            yield return StartCoroutine(LoadAllClipsFromBundle(a => LMGShots = a, "combat/lmg/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => SMGShots = a, "combat/smg/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => RifleShots = a, "combat/rifle/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => PistolShots = a, "combat/pistol/shots"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ShotgunShots = a, "combat/shotgun/shots"));

            Debug.Log("loading casings audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => LMGCasings = a, "combat/lmg/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => SMGCasings = a, "combat/smg/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => RifleCasings = a, "combat/rifle/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => PistolCasings = a, "combat/pistol/casings"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ShotgunCasings = a, "combat/shotgun/casings"));

            Debug.Log("loading misc combat audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => DryFireClips = a, "combat/dryfire"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ArmourBreakClips = a, "combat/armourbreak"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => FootstepClips = a, "combat/footsteps"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ExplosionClips = a, "combat/explosions"));

            Debug.Log("loading weather audio");
            yield return StartCoroutine(LoadAllClipsFromBundle(a => ThunderSounds = a, "thunder"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => LightRainClips = a, "rain/light"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => MediumRainClips = a, "rain/medium"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => HeavyRainClips = a, "rain/heavy"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => LightWindClips = a, "wind/light"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => MediumWindClips = a, "wind/medium"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => HeavyWindClips = a, "wind/heavy"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => NightAudio = a, "nighttime"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => DayAudio = a, "daytime"));
            yield return StartCoroutine(LoadAllClipsFromBundle(a => Ambient = a, "drones"));

            Debug.Log("loading misc audio");
            yield return StartCoroutine(LoadClip(a => ButtonSelectClip = a, "misc/buttonclick", "Button Click"));

            Debug.Log("loading music");
            yield return StartCoroutine(LoadClip(a => SimmavA = a, "music/combat/simmav", "simmav a"));
            yield return StartCoroutine(LoadClip(a => SimmavB = a, "music/combat/simmav", "simmav b"));
            yield return StartCoroutine(LoadClip(a => SimmavC = a, "music/combat/simmav", "simmav c"));

            watch.Stop();
            Helper.PrintTime("Done loading audio in: ", watch);
            _loaded = true;
        }

        public static bool Loaded()
        {
            return _loaded;
        }
    }
}