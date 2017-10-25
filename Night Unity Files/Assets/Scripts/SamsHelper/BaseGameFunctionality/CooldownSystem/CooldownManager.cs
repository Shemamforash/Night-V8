using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class CooldownManager
    {
        private readonly List<Cooldown> _activeCooldowns = new List<Cooldown>();
        
        public void UpdateCooldowns()
        {
            for (int i = _activeCooldowns.Count - 1; i >= 0; --i)
            {
                Cooldown cooldown = _activeCooldowns[i];
                if (cooldown.Update())
                {
                    _activeCooldowns.RemoveAt(i);
                }
            }
        }

        public void RegisterCooldown(Cooldown c)
        {
            _activeCooldowns.Add(c);
        }

        public void RemoveCooldown(Cooldown cooldown)
        {
            _activeCooldowns.Remove(cooldown);
        }
    }
}