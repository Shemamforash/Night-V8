using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Bosses.Starfish;
using Game.Combat.Enemies.Misc;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class StarfishBehaviour : Boss
{
    private const int Attack1Target = 60, Attack2Target = 45, Attack3Target = 35, Attack4Target = 20;
    private readonly List<StarFishMainArmBehaviour> _arms = new List<StarFishMainArmBehaviour>();
    private static float _radiusModifier = 1f;
    private float _timeToContract;
    private static StarfishBehaviour _instance;
    private bool _contracting;
    private StarfishGhoulSpawn _ghoulSpawn;
    private StarfishSpreadFire _spreadFire;
    private SpriteRenderer _shotGlow;
    private Tweener _glowTween;
    private float _sinTime = 0;
    private static bool _playAudio;
    private AudioPoolController _audioPool;

    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Starfish/Starfish");
        Instantiate(prefab).transform.position = new Vector2(0, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
        _ghoulSpawn = gameObject.AddComponent<StarfishGhoulSpawn>();
        _spreadFire = gameObject.AddComponent<StarfishSpreadFire>();
        _shotGlow = gameObject.FindChildWithName<SpriteRenderer>("Shot Glow");
        _audioPool = gameObject.GetComponent<AudioPoolController>();
        _audioPool.SetMixerGroup("Combat", 0.5f);
    }

    public void FlashGlow()
    {
        _glowTween?.Kill();
        _shotGlow.SetAlpha(1f);
        _glowTween = _shotGlow.DOFade(0f, 0.5f);
    }

    public void PlayAudio(float volumeOffset, float pitchOffset, float lpfLimit = 25000)
    {
        InstancedAudio instancedAudio = _audioPool.Create(false);
        float volume = Random.Range(0.9f, 1f) - volumeOffset;
        float pitch = Random.Range(0.9f, 1f) - pitchOffset;
        instancedAudio.SetLowPassCutoff(lpfLimit);
        instancedAudio.Play(AudioClips.StarfishAttack, volume, pitch);
    }

    public void Start()
    {
        for (int i = 1; i < 6; ++i)
        {
            GameObject mainArm = gameObject.FindChildWithName("Main Arm " + i);
            _arms.Add(mainArm.GetComponent<StarFishMainArmBehaviour>());
        }

        PreWarmArms();
    }

    public static StarfishBehaviour Instance()
    {
        return _instance;
    }

    private void PreWarmArms()
    {
        float prewarmTime = 3f;
        float currentTime = Time.timeSinceLevelLoad - prewarmTime;
        int frames = (int) (prewarmTime * 60);
        float interval = prewarmTime / frames;
        for (int i = 0; i < frames; ++i)
        {
            currentTime += interval;
            for (int j = 0; j < _arms.Count; j++)
            {
                StarFishMainArmBehaviour arm = _arms[j];
                float sinTime = Mathf.Sin(currentTime + 0.25f * Mathf.PerlinNoise(currentTime, j));
                float zAngle = sinTime * 30;
                arm.UpdateAngle(zAngle);
            }
        }
    }

    private float GetCurrentAngle(int i)
    {
        float oldSinTime = _sinTime;
        _sinTime = Mathf.Sin(Time.timeSinceLevelLoad);
        if (oldSinTime < 0f && _sinTime >= 0f || oldSinTime > 0f && _sinTime <= 0f) _playAudio = true;
        else _playAudio = false;
        float zAngle = _sinTime * 30;
        return zAngle;
    }

    public static bool ShouldPlayAudio() => _playAudio;

    public static float GetRadiusModifier()
    {
        return _radiusModifier;
    }

    private static void SetRadiusModifier(float radiusModifier)
    {
        _radiusModifier = radiusModifier;
    }

    private void PushPulse()
    {
        PushController.Create(transform.position, 0f, false, 40);
        PushController.Create(transform.position, 72f, false, 40f);
        PushController.Create(transform.position, 144f, false, 40f);
        PushController.Create(transform.position, 216f, false, 40f);
        PushController.Create(transform.position, 288f, false, 40f);
    }

    private void Contract()
    {
        if (_contracting) return;
        if (_timeToContract > 0f) _timeToContract -= Time.deltaTime;
        _contracting = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1.2f, 1f).SetEase(Ease.OutExpo));
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 0.4f, 0.2f).SetEase(Ease.InBack));
        sequence.AppendCallback(PushPulse);
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1f, 3f));
        sequence.AppendCallback(ResetContract);
    }

    private void ResetContract()
    {
        _timeToContract = Random.Range(5f, 6f);
        _contracting = false;
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        _ghoulSpawn.UpdateGhoulSpawn(SectionCount());
        _spreadFire.UpdateSpreadFire();
        Contract();
    }

    public void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, GetCurrentAngle(0));
        for (int i = 0; i < _arms.Count; i++)
        {
            StarFishMainArmBehaviour a = _arms[i];
            a.UpdateAngle(GetCurrentAngle(i));
        }
    }

    public override void UnregisterSection(BossSectionHealthController starFishArmBehaviour)
    {
        int armCountBefore = SectionCount();
        base.UnregisterSection(starFishArmBehaviour);
        int armCountAfter = SectionCount();

        if (armCountBefore > Attack1Target && armCountAfter <= Attack1Target)
            _spreadFire.StartTier1();
        else if (armCountBefore > Attack2Target && armCountAfter <= Attack2Target)
            _spreadFire.StartTier2();
        else if (armCountBefore > Attack3Target && armCountAfter <= Attack3Target)
            _spreadFire.StartTier3();
        else if (armCountBefore > Attack4Target && armCountAfter <= Attack4Target)
            _spreadFire.StartTier4();
    }

    public List<CanTakeDamage> GetSections()
    {
        return new List<CanTakeDamage>(Sections);
    }
}