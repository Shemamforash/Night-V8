using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public abstract class ShrineBehaviour : MonoBehaviour
{
    private const float DistanceToTrigger = 0.2f;
    private bool _triggered;
    private ParticleSystem _essence, _void, _burst, _ring;
    private SpriteRenderer _flash;
    protected SpriteRenderer DangerIndicator;
    private SpriteRenderer _glow;
    protected SpriteMask _countdownMask;
    protected SpriteRenderer _countdown;
    protected readonly List<EnemyBehaviour> _enemiesAlive = new List<EnemyBehaviour>();
    private static GameObject _disappearPrefab;

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
        if (_disappearPrefab == null) _disappearPrefab = Resources.Load<GameObject>("Prefabs/Combat/Disappear");
    }

    protected void Succeed()
    {
        StartCoroutine(SucceedGlow());
    }

    protected void Fail()
    {
        StartCoroutine(FailGlow());
    }

    protected void AddEnemy(EnemyBehaviour b)
    {
        _enemiesAlive.Add(b);
//        b.AddOnKill(e => _enemiesAlive.Remove(e));
    }

    protected bool EnemiesDead()
    {
        List<EnemyBehaviour> enemies = _enemiesAlive.FindAll(e => e == null);
        enemies.ForEach(e => _enemiesAlive.Remove(e));
        return _enemiesAlive.Count == 0;
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

    public void Update()
    {
        if (_triggered) return;
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
        if (distanceToPlayer > DistanceToTrigger) return;
        _triggered = true;
        StartCoroutine(StartShrine());
        StartCoroutine(Flash());
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

    protected abstract IEnumerator StartShrine();

    protected virtual void EndChallenge()
    {
        for (int i = _enemiesAlive.Count - 1; i >= 0; --i)
        {
            Instantiate(_disappearPrefab).transform.position = _enemiesAlive[i].transform.position;
            _enemiesAlive[i].Kill();
        }
    }
}