using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;

namespace Game.Combat
{
    public class CombatScenario
    {
        private DesolationCharacter _character;
        private readonly List<Enemy> _enemies = new List<Enemy>();

        public void Resolve()
        {
            _character.CombatStates.ReturnToDefault();
            _enemies.ForEach(e => e.CombatStates.ReturnToDefault());
        }

        public void Remove(Enemy enemy)
        {
            _enemies.Remove(enemy);
        }

        private void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        public void SetCharacter(DesolationCharacter character)
        {
            _character = character;
        }

        public static CombatScenario Generate(int size)
        {
            CombatScenario scenario = new CombatScenario();
            for (int i = 0; i < size; ++i)
            {
                scenario.AddEnemy(new Enemy("Enemy" + (i == 0 ? "" : i.ToString()), 100, scenario));
            }
            return scenario;
        }

        public DesolationCharacter Character()
        {
            return _character;
        }

        public List<Enemy> Enemies()
        {
            return _enemies;
        }
    }
}