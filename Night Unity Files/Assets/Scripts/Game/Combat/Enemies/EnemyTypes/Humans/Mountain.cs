using System;
using Game.Characters;
using Game.Combat.CharacterUi;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Mountain : EnemyBehaviour
    {
        private bool _firedVolley;

        public override void Initialise(Enemy enemy, EnemyUi characterUi)
        {
            base.Initialise(enemy, characterUi);
//            MinimumFindCoverDistance = 20f;
        }

        public override void ChooseNextAction()
        {
            if (!_firedVolley)
            {
                base.ChooseNextAction();
                return;
            }
            _firedVolley = false;
//            CurrentAction = MoveToTargetPosition(CombatManager.Player.Position.CurrentValue() + 5);
        }
    }
}