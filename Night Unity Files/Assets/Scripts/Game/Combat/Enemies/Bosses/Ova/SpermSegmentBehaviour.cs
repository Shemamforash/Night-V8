﻿using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

public class SpermSegmentBehaviour : CanTakeDamage {
	private SpermBehaviour _spermParent;
	private DamageSpriteFlash _spriteFlash;

	public void Awake()
	{
		base.Awake();
		_spriteFlash = GetComponent<DamageSpriteFlash>();
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}

	public void TakeShotDamage(Shot shot)
	{
		TakeDamage(shot.DamageDealt());
	}

	public void TakeRawDamage(float damage, Vector2 direction)
	{
		TakeDamage(damage);
	}

	public void TakeExplosionDamage(float damage, Vector2 origin, float radius)
	{
		TakeDamage(damage);
	}

	public void Decay()
	{
	}

	private void TakeDamage(float damage)
	{
		_spriteFlash.FlashSprite();
		_spermParent.TakeDamage((int) damage);
	}

	public override string GetDisplayName()
	{
		return "Sperm";
	}

	public void Burn()
	{
	}

	public void Sicken(int stacks = 0)
	{
	}

	public bool IsDead()
	{
		return _spermParent.IsDead();
	}

	public void Kill()
	{
	}

	public void MyUpdate()
	{
	}

	public void SetSperm(SpermBehaviour spermBehaviour)
	{
		_spermParent = spermBehaviour;
	}
}
