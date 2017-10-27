using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class CollectResources : BaseCharacterAction
    {
        private Character _previousCharacter;

        public CollectResources(Player playerCharacter) : base("Collect Resources", playerCharacter)
        {
            IsVisible = false;
            SetStateTransitionTarget("Return");
            AddOnExit(ReturnToGameScreen);
        }

        public override void Enter()
        {
            MenuStateMachine.States.NavigateToState("Inventory Menu");
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = GetCharacter();
            InventoryTransferManager.Instance().ShowInventories(GetCharacter().Inventory(), GetCharacter().CurrentRegion, () => GetCharacter().States.NavigateToState("Return"));
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

        private void ReturnToGameScreen()
        {
            CharacterManager.SelectedCharacter = _previousCharacter;
            MenuStateMachine.GoToInitialMenu();
        }
    }
}