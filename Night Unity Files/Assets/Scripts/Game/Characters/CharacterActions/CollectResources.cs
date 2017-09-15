using SamsHelper.ReactiveUI.InventoryUI;
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
            SetStateTransitionTarget("Return");
            AddOnExit(ReturnToGameScreen);
        }

        public override void Enter()
        {
            MenuStateMachine.Instance().NavigateToState("Inventory Menu");
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = GetCharacter();
            InventoryTransferManager.Instance().ShowDualInventories(GetCharacter().CharacterInventory, GetCharacter().CurrentRegion, () => GetCharacter().NavigateToState("Return"));
        }

        public override void Interrupt()
        {
            base.Interrupt();
            ReturnToGameScreen();
        }

        public override void Resume()
        {
            base.Resume();
            Enter();
        }

        public void ReturnToGameScreen()
        {
            CharacterManager.SelectedCharacter = _previousCharacter;
            MenuStateMachine.Instance().GoToInitialMenu();
        }
    }
}