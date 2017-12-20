namespace Game.Characters.CharacterActions
{
    public class Combat : BaseCharacterAction
    {
        public Combat(Player playerCharacter) : base("Combat", playerCharacter)
        {
            IsVisible = false;
        }

        public override void Enter()
        {
//            CombatManager.EnterCombat();
        }
    }
}