using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class Grenade : CombatItem
{
	private int _damage = 20;

	public Grenade(int distance, int targetDistance, string name = "Grenade") : base(name, 15, targetDistance)
	{
		Distance.SetCurrentValue(distance);
	}

	protected virtual void ReachTarget()
	{
		Shot s = new Shot(null, null);
		s.SetKnockDownRadius(5);
		s.SetDamage(20);
		s.SetKnockdownChance(0.75f);
		s.Fire();
		Kill();
		CombatManager.RemoveGrenade(this);
	}

	public override ViewParent CreateUi(Transform parent)
	{
		EnemyUi = new BasicEnemyView(this, parent);
		Distance.AddOnValueChange(a =>
		{
			float distance = Helper.Round(Distance.CurrentValue());
			string distanceText = distance.ToString() + "m";
			EnemyUi.DistanceText.text = distanceText;
			float normalisedDistance = Helper.Normalise(distance, MaxDistance);
			float alpha = 1f - normalisedDistance;
			alpha *= alpha;
			alpha = Mathf.Clamp(alpha, 0.2f, 1f);
			EnemyUi.SetAlpha(alpha);
		});
		return EnemyUi;
	}

	public void Update()
	{
		MoveToTargetDistance();
	}
}
