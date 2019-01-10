using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public abstract class ShrineBehaviour : BasicShrineBehaviour
{
    private ParticleSystem _essence, _void, _burst, _ring;
    private SpriteRenderer _flash;
    protected SpriteRenderer DangerIndicator;
    private SpriteRenderer _glow;
    private SpriteMask _countdownMask;
    private SpriteRenderer _countdown;
    private Brand _brand;
    public static ShrineBehaviour ActiveShrine;
    private static GameObject _bossPrefab, _firePrefab, _wavePrefab, _chasePrefab;
    private static List<GameObject> _prefabs = new List<GameObject>();
    private float _nextTickTime = -1;
    private AudioSource _audioSource;
    private AudioHighPassFilter _hpfFilter;

    public void Awake()
    {
        ActiveShrine = this;
        _essence = gameObject.FindChildWithName<ParticleSystem>("Essence Cloud");
        _void = gameObject.FindChildWithName<ParticleSystem>("Void");
        _burst = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _ring = gameObject.FindChildWithName<ParticleSystem>("Ring");
        _flash = gameObject.FindChildWithName<SpriteRenderer>("Flash");
        DangerIndicator = gameObject.FindChildWithName<SpriteRenderer>("Danger Indicator");
        _countdown = gameObject.FindChildWithName<SpriteRenderer>("Countdown");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        _countdownMask = gameObject.FindChildWithName<SpriteMask>("Countdown Mask");
        _flash.color = UiAppearanceController.InvisibleColour;
        _countdownMask.alphaCutoff = 1f;
        _audioSource = GetComponent<AudioSource>();
        _hpfFilter = GetComponent<AudioHighPassFilter>();
    }

    public static void Generate(Brand brand)
    {
        if (_bossPrefab == null)
        {
            _bossPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Boss Shrine");
            _firePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fire Shrine");
            _wavePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Wave Shrine");
            _chasePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Chase Shrine");
            _prefabs.Add(_bossPrefab);
            _prefabs.Add(_firePrefab);
            _prefabs.Add(_wavePrefab);
            _prefabs.Add(_chasePrefab);
        }

        GameObject shrine = Instantiate(_prefabs.RandomElement());
        shrine.transform.position = Vector2.zero;
        shrine.transform.localScale = Vector2.one;
        shrine.GetComponent<ShrineBehaviour>()._brand = brand;
    }

    protected override void Succeed()
    {
        base.Succeed();
        _brand.Succeed();
        StartCoroutine(SucceedGlow());
    }

    public override void Fail()
    {
        base.Fail();
        _brand.Fail();
        StartCoroutine(FailGlow());
    }

    private IEnumerator FailGlow()
    {
        float glowTimeMax = 1f;
        float glowTime = glowTimeMax;

        Color startingColour = DangerIndicator.color;
        Color timerStartColor = _countdown.color;
        while (glowTime > 0f)
        {
            if (!CombatManager.IsCombatActive()) yield return null;
            float lerpVal = 1 - glowTime / glowTimeMax;
            Color c = Color.Lerp(startingColour, UiAppearanceController.InvisibleColour, lerpVal);
            DangerIndicator.color = c;
            DangerIndicator.transform.FindAllComponentsInChildren<SpriteRenderer>().ForEach(s => { s.color = c; });
            _countdown.color = Color.Lerp(timerStartColor, UiAppearanceController.InvisibleColour, lerpVal);
            glowTime -= Time.deltaTime;
            yield return null;
        }

        Destroy(_glow.gameObject);
        Destroy(_countdown.gameObject);
        Destroy(DangerIndicator.gameObject);
        Destroy(this);
    }

    private IEnumerator SucceedGlow()
    {
        float glowTimeMax = 1f;

        float glowTime = glowTimeMax;

        _countdownMask.alphaCutoff = 0f;

        while (glowTime > 0f)
        {
            if (!CombatManager.IsCombatActive()) yield return null;
            float lerpVal = 1 - glowTime / glowTimeMax;
            Color c = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, lerpVal);
            DangerIndicator.color = c;
            _glow.color = c;
            DangerIndicator.transform.FindAllComponentsInChildren<SpriteRenderer>().ForEach(s => { s.color = c; });
            _countdown.color = c;
            glowTime -= Time.deltaTime;
            yield return null;
        }

        Destroy(_glow.gameObject);
        Destroy(_countdown.gameObject);
        Destroy(DangerIndicator.gameObject);
        Destroy(this);
    }

    protected void UpdateCountdown(float currentTime, float maxTime, bool reset = false)
    {
        float normalisedTime = currentTime / maxTime;
        if (_nextTickTime < 0 || reset) _nextTickTime = currentTime - 1f;
        if (currentTime < _nextTickTime)
        {
            _nextTickTime = currentTime - 1f;
            if (_nextTickTime > 0f)
            {
                float volume = Mathf.Lerp(0.5f, 0f, normalisedTime);
                float hpfValue = Mathf.Lerp(10000, 100, normalisedTime);
                _hpfFilter.cutoffFrequency = hpfValue;
                _audioSource.volume = volume;
                _audioSource.Play();
            }
        }

        _countdownMask.alphaCutoff = 1 - normalisedTime;
        _countdown.color = new Color(1, normalisedTime, normalisedTime, _countdown.color.a);
    }

    private IEnumerator Flash()
    {
        float time = 0.2f;
        _burst.Emit(50);
        Destroy(_essence.gameObject);
        Destroy(_void.gameObject);
        Destroy(_ring.gameObject);
        _flash.color = Color.white;
        while (time > 0f)
        {
            if (!CombatManager.IsCombatActive()) yield return null;
            float lerpVal = 1 - (time / 0.2f);
            _flash.color = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, lerpVal);
            time -= Time.deltaTime;
            yield return null;
        }

        _flash.color = UiAppearanceController.InvisibleColour;
    }

    protected override void StartShrine()
    {
        Triggered = true;
        ShowShrineInstructions();
        StartCoroutine(Flash());
    }

    protected abstract void StartChallenge();

    protected abstract string GetInstructionText();

    private void ShowShrineInstructions()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => EventTextController.SetOverrideText(GetInstructionText()));
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            EventTextController.CloseOverrideText();
            StartChallenge();
        });
    }

    protected virtual void EndChallenge()
    {
        ScreenFaderController.FlashWhite(1f, new Color(1, 1, 1, 0f));
        RiteStarter.Generate(null);
        End();
    }
}