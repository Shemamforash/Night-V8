using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality
{
    public static class CooldownManager
    {
        private static List<Cooldown> activeCooldowns = new List<Cooldown>();
        
        public static void UpdateCooldowns()
        {
            for (int i = activeCooldowns.Count - 1; i >= 0; --i)
            {
                Cooldown cooldown = activeCooldowns[i];
                if (cooldown.Update())
                {
                    activeCooldowns.RemoveAt(i);
                }
            }
        }

        public static void RegisterCooldown(Cooldown c)
        {
            activeCooldowns.Add(c);
        }
    }
}