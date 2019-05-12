using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
	public class CooldownManager
	{
		private readonly List<Cooldown> _activeCooldowns = new List<Cooldown>();

		public Cooldown CreateCooldown(float duration = 0) => new Cooldown(this, duration);

		public void UpdateCooldowns()
		{
			for (int i = _activeCooldowns.Count - 1; i >= 0; --i)
			{
				Cooldown cooldown = _activeCooldowns[i];
				cooldown.Update();
				if (cooldown.Running()) continue;
				_activeCooldowns.RemoveAt(i);
			}
		}

		public void RegisterCooldown(Cooldown c)
		{
			_activeCooldowns.Add(c);
		}

		public void Clear()
		{
			_activeCooldowns.ForEach(a => a.Cancel());
		}
	}
}