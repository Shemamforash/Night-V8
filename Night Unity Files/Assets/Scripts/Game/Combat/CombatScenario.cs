using System.Collections.Generic;
using Game.Characters;

namespace Game.Combat
{
    public class CombatScenario
    {
        public readonly DesolationCharacter Character;
        public readonly List<DesolationCharacter> Enemies;

        public void Resolve()
        {
            Character.CombatStates.ReturnToDefault();
            Enemies.ForEach(e => e.CombatStates.ReturnToDefault());
        }
    }
}