using System;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class Grenade : CombatItem
{
	private int _damage = 20;
	public BasicEnemyView GrenadeView;
	private Action _moveAction;
	
	public Grenade(float position, float targetPosition, string name = "Grenade") : base(name, position)
	{
		_moveAction = MoveToTargetPosition(targetPosition);
		MovementController.SetSpeed(15);
	}

	protected override void ReachTarget()
	{
		CreateExplosion();
		Kill();
		UIGrenadeController.RemoveGrenade(this);
	}

	protected virtual void CreateExplosion()
	{
		Explosion explosion = new Explosion(Position.CurrentValue(), 5, 20);
		explosion.SetKnockbackDistance(5);
		explosion.Fire();
	}

	public override ViewParent CreateUi(Transform parent)
	{
		GrenadeView = new BasicEnemyView(this, parent);
		GrenadeView.SetNavigatable(false);
		SetDistanceData(GrenadeView);
		GrenadeView.SetNavigatable(false);
		return GrenadeView;
	}

	public void SetTargetPosition(float targetPosition)
	{
		_moveAction = MoveToTargetPosition(targetPosition);
	}
	
	public override void UpdateCombat()
	{
		_moveAction();
	}
}
