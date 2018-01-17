using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class Grenade : CombatItem
{
	private int _damage = 20;
	public BasicEnemyView GrenadeView;
	
	public Grenade(float distance, float targetPosition, string name = "Grenade") : base(name, 15, targetPosition)
	{
		Position.SetCurrentValue(distance);
	}

	protected override void ReachTarget()
	{
		CreateExplosion();
		Kill();
		CombatManager.RemoveGrenade(this);
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
		SetDistanceData(GrenadeView);
		GrenadeView.SetNavigatable(false);
		return GrenadeView;
	}

	public void Update()
	{
		MoveToTargetDistance();
	}
}
