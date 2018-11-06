using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Shadow : NightmareEnemyBehaviour
    {
        public void Start()
        {
            gameObject.AddComponent<ErraticDash>();
            if (Helper.RollDie(0, 2)) gameObject.AddComponent<Push>().Initialise(4, 2);
            else gameObject.AddComponent<Needler>().Initialise(1);
        }
    }
}