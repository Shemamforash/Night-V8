using Facilitating.UIControllers;

namespace Game.Characters.CharacterActions
{
    public class Meditate : BaseCharacterAction
    {
        public Meditate(Player playerCharacter) : base(nameof(Meditate), playerCharacter)
        {
            DisplayName = "Meditating";
            HourCallback = Exit;
        }

        protected override void OnClick()
        {
            UiGearMenuController.ShowMeditateMenu();
        }
    }
}