using Facilitating.UIControllers;
using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class Craft : BaseCharacterAction
    {
        private Recipe _recipe;

        public Craft(Player playerCharacter) : base("Craft", playerCharacter)
        {
            DisplayName = "Crafting";
            HourCallback = CraftRecipe;
            MinuteCallback = () => --Duration;
        }

        protected override void OnClick()
        {
            UiGearMenuController.ShowCraftingMenu();
        }

        private void CraftRecipe()
        {
            _recipe.Craft();
            PlayerCharacter.RestAction.Enter();
        }

        public void StartCrafting(Recipe recipe)
        {
            _recipe = recipe;
            _recipe.ConsumeResources();
            SetDuration(WorldState.MinutesPerHour);
            Enter();
        }
    }
}