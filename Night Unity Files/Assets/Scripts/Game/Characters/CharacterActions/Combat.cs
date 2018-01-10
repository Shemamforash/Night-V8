namespace Game.Characters.CharacterActions
{
    public class Combat : BaseCharacterAction
    {
        public Combat(Player.Player playerCharacter) : base("Combat", playerCharacter)
        {
            IsVisible = false;
        }

        public override void Enter()
        {
            base.Enter();
//            CombatManager.EnterCombat();
        }
    }
}