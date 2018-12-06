using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Shadow : NightmareEnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<ErraticDash>();
            gameObject.AddComponent<Push>().Initialise(4, 2);
            if (WorldState.Difficulty() > 15) gameObject.AddComponent<Needler>().Initialise(1);
        }
    }
}