using Facilitating.UIControllers;

namespace Game.Characters.CharacterActions
{
    public class Craft : BaseCharacterAction
    {
        public Craft(Player playerCharacter) : base("Craft", playerCharacter)
        {
            DisplayName = "Crafting";
            HourCallback = Exit;
        }

        protected override void OnClick()
        {
            UiGearMenuController.ShowCraftingMenu();
        }
    }
}