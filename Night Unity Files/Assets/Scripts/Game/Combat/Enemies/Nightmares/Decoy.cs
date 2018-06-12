using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Decoy : EnemyBehaviour
    {
        private string _decoyName;
        
        public override string GetEnemyName()
        {
            return _decoyName;
        }

        public static EnemyBehaviour Create(EnemyBehaviour origin)
        {
            Decoy decoy = (Decoy)CombatManager.QueueEnemyToAdd(EnemyType.Decoy);
            decoy._decoyName = origin.GetEnemyName();
            decoy.GetComponent<SpriteRenderer>().sprite = origin.GetComponent<SpriteRenderer>().sprite;
            decoy.Speed = origin.Speed;
            return decoy;
        }
    }
}