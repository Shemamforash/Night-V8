using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using UnityEngine;

public class WormSacBehaviour : BossSectionHealthController
{
    private SpriteRenderer _sacSprite;
    private CircleCollider2D _collider;
    private float _currentScaleFactor;
    private const float InflateDuration = 5f;
    private float _noiseOffset;

    public override void Awake()
    {
        base.Awake();
        _sacSprite = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
        _noiseOffset = Random.Range(0f, 1000000f);
        StartCoroutine(ScaleUp());
    }

    protected override int GetInitialHealth()
    {
        return 20;
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        WormBehaviour.TakeDamage(damage);
    }
    
    public override void Start()
    {
        SetBoss(WormBehaviour.Instance());
        base.Start();
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
        for (int i = 0; i < Random.Range(4, 7); ++i)
        {
            EnemyBehaviour enemy = CombatManager.SpawnEnemy(EnemyType.Ghast, transform.position);
            enemy.MovementController.Knockback(transform.position, Random.Range(20f, 30f));
        }
    }

    public void Update()
    {
        float noisyScaleFactor = Mathf.PerlinNoise(Time.timeSinceLevelLoad, _noiseOffset);
        noisyScaleFactor = noisyScaleFactor / 2f + 0.5f;
        float scaleFactor = noisyScaleFactor * _currentScaleFactor;
        _collider.radius = scaleFactor;
        _sacSprite.color = new Color(1, 1, 1, scaleFactor);
        transform.localScale = Vector2.one * scaleFactor;
    }

    public void Initialise(Vector2 position)
    {
        transform.position = position;
    }
}