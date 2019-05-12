using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public abstract class BasicShrineBehaviour : MonoBehaviour
	{
		private static GameObject _disappearPrefab;
		protected      bool       IsInRange;
		protected      bool       Triggered;

		public void OnTriggerEnter2D(Collider2D other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			IsInRange = true;
			if (Triggered) return;
			StartShrine();
		}

		public void OnTriggerExit2D(Collider2D other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			IsInRange = false;
		}

		protected virtual void Succeed()
		{
			End();
		}

		public virtual void Fail()
		{
			End();
		}

		protected void End()
		{
			CombatManager.Instance().ClearInactiveEnemies();
			for (int i = CombatManager.Instance().Enemies().Count - 1; i >= 0; --i)
			{
				if (_disappearPrefab == null) _disappearPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Disappear");
				Instantiate(_disappearPrefab).transform.position = CombatManager.Instance().Enemies()[i].transform.position;
				CombatManager.Instance().Enemies()[i].Kill();
			}
		}

		protected abstract void StartShrine();
	}
}