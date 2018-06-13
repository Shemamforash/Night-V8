using System.Collections;
using System.Collections.Generic;
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
    private readonly List<EnemyBehaviour> _enemiesAlive = new List<EnemyBehaviour>();

    public void Awake()
    {
        _essence = Helper.FindChildWithName<ParticleSystem>(gameObject, "Essence Cloud");
        _void = Helper.FindChildWithName<ParticleSystem>(gameObject, "Void");
        _burst = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burst");
        _ring = Helper.FindChildWithName<ParticleSystem>(gameObject, "Ring");
        _flash = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Flash");
        DangerIndicator = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Danger Indicator");
        _countdown= Helper.FindChildWithName<SpriteRenderer>(gameObject, "Countdown");
        _glow = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Glow");
        _countdownMask = Helper.FindChildWithName<SpriteMask>(gameObject, "Countdown Mask");
        _flash.color = UiAppearanceController.InvisibleColour;
        _countdownMask.alphaCutoff = 1f;
    }

    protected void Succeed()
    {
        StartCoroutine(SucceedGlow());
    }

    protected EnemyBehaviour SpawnEnemy(EnemyType enemyType, Vector2 position)
    {
        EnemyBehaviour enemy = CombatManager.QueueEnemyToAdd(enemyType);
        _enemiesAlive.Add(enemy);
        enemy.AddOnKill(e => _enemiesAlive.Remove(e));
        enemy.transform.position = position;
        TeleportInOnly.TeleportObjectIn(enemy.gameObject);
        return enemy;
    }

    protected bool EnemiesDead()
    {
        return _enemiesAlive.Count == 0;
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
        _countdownMask.alphaCutoff = 1 - currentTime / maxTime;
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
}