using Game.Combat.Enemies.EnemyBehaviours;
using Game.Gear.Weapons;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Fighter : Enemy
    {
        public Fighter() : base("Fighter", Random.Range(2000, 4000))
        {
            Equip(WeaponGenerator.GenerateWeapon());
        }
        
//        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
//        {
//            base.InitialiseBehaviour(relation);
//            BasicFire fire = new BasicFire(relation);
//            Wander wander = new Wander(relation);
//            wander.AddExitTransition(wander);
//            Flee flee = new Flee(relation);
//            wander.SetOnDetectBehaviour(fire);
//            fire.AddExitTransition(flee);
//            GenericBehaviour behaviour = new GenericBehaviour(relation);
//            EnemyBehaviour.NavigateToState(behaviour.Name);
//        }
    }
}