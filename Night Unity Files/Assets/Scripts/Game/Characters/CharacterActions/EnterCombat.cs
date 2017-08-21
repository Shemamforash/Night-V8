using Characters;
using Game.Combat;

namespace Game.Characters.CharacterActions
{
    public class EnterCombat : BaseCharacterAction
    {
        public EnterCombat(Character character) : base("Enter Combat", character)
        {
            IsDurationFixed = true;
            _defaultDuration = 0;
        }

        public override void Exit()
        {
            base.Exit();
            CombatManager.EnterCombat(Character);
        }
    }
}