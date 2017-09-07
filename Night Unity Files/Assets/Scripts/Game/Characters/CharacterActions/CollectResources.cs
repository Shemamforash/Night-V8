using Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class CollectResources : BaseCharacterAction
    {
        private Character _previousCharacter;
        
        public CollectResources(Character character) : base("Collect Resources", character)
        {
            IsVisible = false;
        }

        public override void Enter()
        {
            MenuStateMachine.Instance.NavigateToState("Pick Up Menu");
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = GetCharacter();
            InventoryManager inventoryManager = GameObject.Find("Pick Up Menu").GetComponent<InventoryManager>();
            inventoryManager.SetInventories(GetCharacter().CharacterInventory, GetCharacter().CurrentRegion, Exit);
        }

        public override void Exit()
        {
            CharacterManager.SelectedCharacter = _previousCharacter;
            MenuStateMachine.Instance.GoToInitialMenu();
            ParentMachine.NavigateToState("Return");
        }
    }
}