using System;
using System.Collections.Generic;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UIGrenadeController : MonoBehaviour {
		public static readonly List<Grenade> Grenades = new List<Grenade>();
		private static UIGrenadeController _instance;

		private void Awake()
		{
			_instance = this;
		}

		public static void RemoveGrenade(Grenade g)
		{
			Grenades.Remove(g);
			Destroy(g);
		}
	
		public static void AddGrenade(MiscEnemyType type, float position, float targetPosition)
		{
			GameObject grenadeGameObject = Helper.InstantiateUiObject("Prefabs/Inventory/OtherCombatItem", _instance.transform);
			Grenade combatGrenade;
			switch (type)
			{
				case MiscEnemyType.Grenade:
					combatGrenade = grenadeGameObject.AddComponent<Grenade>();
					break;
				case MiscEnemyType.Incendiary:
					combatGrenade = grenadeGameObject.AddComponent<IncendiaryGrenade>();
					break;
				case MiscEnemyType.Splinter:
					combatGrenade = grenadeGameObject.AddComponent<SplinterGrenade>();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			combatGrenade.SetTargetPosition(position, targetPosition);
			Grenades.Add(combatGrenade);
		}
	}
}
