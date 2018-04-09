using Facilitating.UIControllers;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class CraftAmmo : BaseCharacterAction
    {
        private InventoryResourceType _magType;

        public CraftAmmo(Player playerCharacter) : base("Craft Ammo", playerCharacter)
        {
            DisplayName = "Crafting";
            HourCallback = () =>
            {
                WorldState.HomeInventory().IncrementResource(_magType, 1);
                Exit();
            };
        }

        protected override void OnClick()
        {
            UiCreateAmmoController.Instance().ShowMenu(PlayerCharacter);
        }

        public void SetAmmoType(InventoryResourceType magType)
        {
            _magType = magType;
            MenuStateMachine.GoToInitialMenu();
            Enter();
        }
    }
}