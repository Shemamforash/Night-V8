using DG.Tweening;
using Game.Combat.Misc;
using UnityEngine;

public class WingHealthScript : MonoBehaviour, ITakeDamageInterface
{
	private const int InitialWingHealth = 10;
	private readonly HealthController _healthController = new HealthController();
	private SpriteRenderer _sprite;

	public void Awake()
	{
		_sprite = GetComponent<SpriteRenderer>();
		_healthController.SetInitialHealth(InitialWingHealth, this);
	}

	public void Start()
	{
		SerpentBehaviour.RegisterWing(this);
	}

	private void TakeDamage(float damage)
	{
		_sprite.color = Color.red;
		_sprite.DOColor(Color.white, 0.5f);
		_healthController.TakeDamage(damage);
	}
	
	public void TakeShotDamage(Shot shot)
	{
		TakeDamage(shot.DamageDealt());
	}

	public void TakeRawDamage(float damage, Vector2 direction)
	{
		TakeDamage(damage);
	}

	public void TakeExplosionDamage(float damage, Vector2 origin)
	{
		TakeDamage(damage);
	}

	public void Decay()
	{
	}

	public void Burn()
	{
	}

	public void Sicken(int stacks = 0)
	{
	}

	public bool IsDead()
	{
		return _healthController.GetCurrentHealth() == 0;
	}

	public void Kill()
	{
		Destroy(gameObject);
		SerpentBehaviour.UnregisterWing(this);
	}
}
