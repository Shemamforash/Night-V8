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

    protected override void Awake()
    {
        base.Awake();
        _sacSprite = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
        _noiseOffset = Random.Range(0f, 1000000f);
        StartCoroutine(ScaleUp());
    }

    protected override int GetInitialHealth()
    {
        return 100;
    }

    private void TakeDamage(float damage)
    {
        float healthBefore = HealthController.GetCurrentHealth();
        float healthAfter = Mathf.Clamp(healthBefore - damage, 0, 100000);
        int difference = (int) (healthBefore - healthAfter);
        WormBehaviour.TakeDamage(difference);
    }

    public override void TakeShotDamage(Shot s)
    {
        TakeDamage(s.DamageDealt());
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
        CombatManager.Remove(this);
        WormBehaviour.ReturnSac(this);
    }

    public override string GetDisplayName()
    {
        return "Sac";
    }

    public void Start()
    {
        SetBoss(WormBehaviour.Instance());
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
        WormBehaviour.ReturnSac(this);
        List<EnemyTemplate> validEnemies = WorldState.GetAllowedNightmareEnemyTypes();
        List<EnemyType> typesToAdd = new List<EnemyType>();
        int size = Random.Range(4, 7);
        while (size > 0)
        {
            validEnemies.Shuffle();
            foreach (EnemyTemplate template in validEnemies)
            {
                if (template.Value > size) return;
                size -= template.Value;
                typesToAdd.Add(template.EnemyType);
                break;
            }
        }

        foreach (EnemyType enemyType in typesToAdd)
        {
            EnemyBehaviour enemy = CombatManager.SpawnEnemy(enemyType, transform.position);
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
        _collider.radius = scaleFactor;
        transform.localScale = Vector2.one * scaleFactor;
    }

    public void Initialise(Vector2 position)
    {
        transform.position = position;
    }
}