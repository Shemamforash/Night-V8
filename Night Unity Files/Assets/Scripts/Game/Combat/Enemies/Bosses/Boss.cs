using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
	public class Boss : MonoBehaviour
	{
		public readonly List<BossSectionHealthController> Sections = new List<BossSectionHealthController>();
		protected       Rigidbody2D                       RigidBody;

		protected virtual void Awake()
		{
			gameObject.layer = 24;
			RigidBody        = GetComponent<Rigidbody2D>();
			Tomb.TombActive  = true;
		}

		public void RegisterSection(BossSectionHealthController section)
		{
			Sections.Add(section);
		}

		public virtual void UnregisterSection(BossSectionHealthController section)
		{
			int countBefore = Sections.Count;
			Sections.Remove(section);
			if (Sections.Count == 0 && countBefore > 0) Kill();
		}

		public virtual void Kill()
		{
			ScreenFaderController.FlashWhite(1f, new Color(1, 1, 1, 0f));
			List<CanTakeDamage> enemies = CombatManager.Instance().Enemies();
			for (int i = enemies.Count - 1; i >= 0; --i) enemies[i].Kill();
			Destroy(gameObject);
			BossDeathController.Create(transform.position);
			RiteStarter.GenerateNextEnvironmentPortal();
			Tomb.TombActive = false;
		}

		protected int SectionCount() => Sections.Count;
	}
}