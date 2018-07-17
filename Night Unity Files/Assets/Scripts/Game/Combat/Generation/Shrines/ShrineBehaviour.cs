using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
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
    private BrandManager.Brand _brand;

    private static GameObject _bossPrefab, _firePrefab, _wavePrefab, _chasePrefab;

    public void Awake()
    {
        _essence = Helper.FindChildWithName<ParticleSystem>(gameObject, "Essence Cloud");
        _void = Helper.FindChildWithName<ParticleSystem>(gameObject, "Void");
        _burst = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burst");
        _ring = Helper.FindChildWithName<ParticleSystem>(gameObject, "Ring");
        _flash = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Flash");
        DangerIndicator = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Danger Indicator");
        _countdown = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Countdown");
        _glow = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Glow");
        _countdownMask = Helper.FindChildWithName<SpriteMask>(gameObject, "Countdown Mask");
        _flash.color = UiAppearanceController.InvisibleColour;
        _countdownMask.alphaCutoff = 1f;
    }

    public static void Generate(ShrineType shrineType, BrandManager.Brand brand)
    {
        if (_bossPrefab == null)
        {
            _bossPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Boss Shrine");
            _firePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fire Shrine");
            _wavePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Wave Shrine");
            _chasePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Chase Shrine");
        }

        GameObject shrine = null;
        switch (shrineType)
        {
            case ShrineType.Wave:
                shrine = Instantiate(_wavePrefab);
                break;
            case ShrineType.Fire:
                shrine = Instantiate(_firePrefab);
                break;
            case ShrineType.Chase:
                shrine = Instantiate(_chasePrefab);
                break;
            case ShrineType.Boss:
                shrine = Instantiate(_bossPrefab);
                break;
        }

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

    protected override void Fail()
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
            float lerpVal = 1 - glowTime / glowTimeMax;
            Color c = Color.Lerp(startingColour, UiAppearanceController.InvisibleColour, lerpVal);
            DangerIndicator.color = c;
            Helper.FindAllComponentsInChildren<SpriteRenderer>(DangerIndicator.transform).ForEach(s => { s.color = c; });
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
            float lerpVal = 1 - glowTime / glowTimeMax;
            Color c = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, lerpVal);
            DangerIndicator.color = c;
            _glow.color = c;
            Helper.FindAllComponentsInChildren<SpriteRenderer>(DangerIndicator.transform).ForEach(s => { s.color = c; });
            _countdown.color = c;
            glowTime -= Time.deltaTime;
            yield return null;
        }

        Destroy(_glow.gameObject);
        Destroy(_countdown.gameObject);
        Destroy(DangerIndicator.gameObject);
        Destroy(this);
    }

    protected void UpdateCountdown(float currentTime, float maxTime)
    {
        float normalisedTime = currentTime / maxTime;
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
            float lerpVal = 1 - (time / 0.2f);
            _flash.color = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, lerpVal);
            time -= Time.deltaTime;
            yield return null;
        }

        _flash.color = UiAppearanceController.InvisibleColour;
    }

    protected override void StartShrine()
    {
        StartCoroutine(Flash());
    }

    protected virtual void EndChallenge()
    {
        End();
    }
}