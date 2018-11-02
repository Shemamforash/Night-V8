using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Shadow : NightmareEnemyBehaviour
    {
        public void Start()
        {
            gameObject.AddComponent<ErraticDash>();
            gameObject.AddComponent<Push>().Initialise(4, 2);
        }
    }
}