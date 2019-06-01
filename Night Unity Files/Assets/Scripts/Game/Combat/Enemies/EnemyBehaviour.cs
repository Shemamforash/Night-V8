using System;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
	public class EnemyBehaviour : CharacterCombat
	{
		public Action        CurrentAction;
		public Enemy         Enemy;
		public MoveBehaviour MoveBehaviour;

		public override Weapon Weapon() => null;

		public override void MyUpdate()
		{
			base.MyUpdate();
			UpdateRotation();
			CurrentAction?.Invoke();
		}

		public override string GetDisplayName() => Enemy.Name;

		protected virtual void UpdateRotation()
		{
			Vector2 targetPosition;
			if (GetTarget() != null) targetPosition = TargetPosition();
			else targetPosition                     = transform.position + (Vector3) GetComponent<Rigidbody2D>().velocity;
			float targetRotation = AdvancedMaths.AngleFromUp(transform.position, targetPosition);
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetRotation));
		}

		public virtual void Initialise(Enemy enemy)
		{
			Enemy = enemy;
			InitialiseAttributes();
			SetTarget(PlayerCombat.Instance);
			SetPosition();
			AssignSprite();
		}

		private void InitialiseAttributes()
		{
			ArmourController = Enemy.Armour;
			ArmourController.Reset();
			float speed = Enemy.Template.Speed;
			speed = Random.Range(speed * 0.8f, speed * 1.2f);
			MovementController.SetSpeed(speed);
			HealthController.SetInitialHealth(Enemy.GetHealth(), this);
			MoveBehaviour = gameObject.AddComponent<MoveBehaviour>();
		}

		private void SetPosition()
		{
			transform.SetParent(GameObject.Find("World").transform);
			Cell c           = WorldGrid.GetCellNearMe(PlayerCombat.Position(), 10f, 7f);
			if (c == null) c = WorldGrid.GetCellNearMe(PlayerCombat.Position(), 20f, 5f);
			transform.position = c.Position;
		}

		private void AssignSprite()
		{
			Sprite spriteImage = Resources.Load<Sprite>("Images/Enemy Symbols/" + GetDisplayName());
			if (spriteImage == null) return;
			Sprite.sprite = spriteImage;
		}

		public override void Kill()
		{
			if (gameObject == null) return;
			Characters.Player player = PlayerCombat.Instance.Player;
			if (player.Attributes.SpreadVoid && VoidStacks > 0)
			{
				CombatManager.Instance().GetCharactersInRange(transform.position, 3).ForEach(c =>
				{
					EnemyBehaviour b = c as EnemyBehaviour;
					if (b == null) return;
					b.Void();
				});
			}

			Loot loot = Enemy.DropLoot(transform.position);
			loot?.CreateObject(true);
			base.Kill();
		}
	}
}