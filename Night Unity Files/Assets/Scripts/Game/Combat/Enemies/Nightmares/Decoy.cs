using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Decoy : NightmareEnemyBehaviour
    {
        private string _decoyName;

        public override string GetDisplayName()
        {
            return _decoyName;
        }

        public static EnemyBehaviour Create(EnemyBehaviour origin)
        {
            Decoy decoy = (Decoy) EnemyTemplate.Create(EnemyType.Decoy);
            decoy._decoyName = origin.GetDisplayName();
            decoy.GetComponent<SpriteRenderer>().sprite = origin.GetComponent<SpriteRenderer>().sprite;
            decoy.MovementController.SetSpeed(origin.Enemy.Template.Speed);
            return decoy;
        }
    }
}