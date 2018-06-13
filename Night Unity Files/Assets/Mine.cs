using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

public class Mine : MonoBehaviour
{
    private float _blinkTimer;
    private const float BlinkTimerMax = 1f;
    private bool _red;
    private SpriteRenderer _spriteRenderer;
    private float _inactiveTime = 2f;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Update()
    {
        if (_inactiveTime > 0)
        {
            _inactiveTime -= Time.deltaTime;
            return;
        }
        float blinkTimeModifier = 1f;
        EnemyBehaviour nearestEnemy = CombatManager.NearestEnemy();
        float nearestEnemyDistance = 5f;
        if(nearestEnemy != null) nearestEnemyDistance = Vector2.Distance(nearestEnemy.transform.position, transform.position);
        float player = Vector2.Distance(PlayerCombat.Instance.transform.position, transform.position);
        float nearestCharacter = Mathf.Min(nearestEnemyDistance, player);
        if (nearestCharacter < 2f)
        {
            blinkTimeModifier = nearestCharacter / 2f;
        }
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= BlinkTimerMax * blinkTimeModifier)
        {
            ChangeColour();
            _blinkTimer = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_inactiveTime > 0) return;
        Explosion.CreateExplosion(transform.position, 20).Detonate();
        Destroy(gameObject);
    }

    private void ChangeColour()
    {
        _red = !_red;
        _spriteRenderer.color = _red ? Color.red : Color.white;
    }
}
