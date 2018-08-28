using Facilitating.UIControllers;

namespace Game.Characters.CharacterActions
{
    public class Consume : BaseCharacterAction
    {
        public Consume(Player playerCharacter) : base("Consume", playerCharacter)
        {
            DisplayName = "Consume";
            HourCallback = Exit;
        }

        protected override void OnClick()
        {
            UiGearMenuController.ShowConsumableMenu();
        }
    }
}