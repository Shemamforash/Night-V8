using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class WormSacBehaviour : BossSectionHealthController
{
    private SpriteRenderer _sacSprite;
    private CircleCollider2D _collider;
    private float _currentScaleFactor;
    private const float InflateDuration = 10f;
    private const float MaxScaleFactor = 3f;
    private float _noiseOffset;
    private float _damageToDie = 200;
    private static GameObject _prefab;
    private static Transform _parent;

    protected override void Awake()
    {
        base.Awake();
        _sacSprite = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
        _noiseOffset = Random.Range(0f, 1000000f);
        StartCoroutine(ScaleUp());
    }

    public static WormSacBehaviour Create(WormBehaviour wormBehaviour)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Worm/Worm Sac");
        if (_parent == null)
        {
            _parent = new GameObject().transform;
            _parent.SetAsDynamicChild();
        }

        GameObject sac = Instantiate(_prefab);
        sac.transform.SetParent(_parent);
        WormSacBehaviour sacBehaviour = sac.GetComponent<WormSacBehaviour>();
        sacBehaviour.SetBoss(wormBehaviour);
        sacBehaviour.SetHealth();
        return sacBehaviour;
    }

    protected override int GetInitialHealth()
    {
        return 100;
    }


    private void SetHealth()
    {
        HealthController.SetInitialHealth(((WormBehaviour) Parent).CurrentHealth(), this, ((WormBehaviour) Parent).MaxHealth());
    }

    private void TakeDamage(float damage)
    {
        float healthBefore = HealthController.GetCurrentHealth();
        float healthAfter = Mathf.Clamp(healthBefore - damage, 0, 100000);
        int difference = (int) (healthBefore - healthAfter);
        if (Parent != null) ((WormBehaviour) Parent).TakeDamage(difference);
        _damageToDie -= damage;
        SetHealth();
        if (_damageToDie > 0) return;
        Kill();
    }

    public override void TakeShotDamage(Shot s)
    {
        TakeDamage(s.Attributes().DamageDealt());
        base.TakeShotDamage(s);
    }

    public override void TakeRawDamage(int damage, Vector2 direction)
    {
        TakeDamage(damage);
        base.TakeRawDamage(damage, direction);
    }

    public override void TakeExplosionDamage(int damage, Vector2 origin, float radius)
    {
        TakeDamage(damage);
        base.TakeExplosionDamage(damage, origin, radius);
    }

    public override void Kill()
    {
        base.Kill();
        Parent.UnregisterSection(this);
    }

    public override string GetDisplayName()
    {
        return "Spawn of Coropthynos";
    }

    private IEnumerator ScaleUp()
    {
        transform.localScale = Vector2.zero;
        float duration = 0f;
        while (duration < InflateDuration)
        {
            duration += Time.deltaTime;
            _currentScaleFactor = duration / InflateDuration;
            yield return null;
        }

        _currentScaleFactor = 1f;
        Pop();
    }

    private void Pop()
    {
        Kill();
        Parent.UnregisterSection(this);
        List<EnemyType> validEnemies = WorldState.GetAllowedNightmareEnemyTypes();
        int size = Random.Range(4, 7);
        List<EnemyType> typesToAdd = EnemyTemplate.RandomiseEnemiesToSize(validEnemies, size);
        foreach (EnemyType enemyType in typesToAdd)
        {
            EnemyBehaviour enemy = CombatManager.Instance().SpawnEnemy(enemyType, transform.position);
            Vector2 direction = (enemy.transform.position - transform.position).normalized;
            enemy.MovementController.KnockBack(direction, Random.Range(20f, 30f));
        }
    }

    public void Update()
    {
        float noisyScaleFactor = Mathf.PerlinNoise(Time.timeSinceLevelLoad, _noiseOffset);
        noisyScaleFactor = noisyScaleFactor / 2f + 0.5f;

        float normalisedScaleFactor = noisyScaleFactor * _currentScaleFactor;
        _sacSprite.color = new Color(1, 1, 1, normalisedScaleFactor);

        float scaleFactor = normalisedScaleFactor * MaxScaleFactor;
        _collider.radius = scaleFactor * 0.15f;
        transform.localScale = Vector2.one * scaleFactor;
    }

    public void Initialise(Vector2 position)
    {
        transform.position = position;
    }
}