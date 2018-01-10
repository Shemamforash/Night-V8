using Game.World;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class CraftAmmo : BaseCharacterAction
    {
        private InventoryResourceType _magType;

        public CraftAmmo(Player.Player playerCharacter) : base("Craft Ammo", playerCharacter)
        {
        }

        public override void Enter()
        {
            base.Enter();
            UICreateAmmoController.Instance().ShowMenu(GetCharacter());
        }
        
        public void SetAmmoType(InventoryResourceType magType)
        {
            _magType = magType;
            SetDuration(1);
            Start();
            MenuStateMachine.GoToInitialMenu();
        }

        public override string GetCostAsString()
        {
            return "Crafting " + _magType + " magazine";
        }
        
        public override void Exit()
        {
            WorldState.HomeInventory().IncrementResource(_magType, 1);
        }
    }
}