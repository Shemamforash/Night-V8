using Facilitating.UIControllers;

namespace Game.Characters.CharacterActions
{
	public class Consume : BaseCharacterAction
	{
		public Consume(Player playerCharacter) : base("Inventory", playerCharacter)
		{
			DisplayName  = "Inventory";
			HourCallback = Exit;
		}

		protected override void OnClick()
		{
			UiGearMenuController.ShowConsumableMenu();
		}
	}
}