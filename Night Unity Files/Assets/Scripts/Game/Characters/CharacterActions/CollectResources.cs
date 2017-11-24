using Game.World.Region;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class CollectResources : BaseCharacterAction
    {
        private Character _previousCharacter;
        private Region _targetRegion;

        public CollectResources(Player playerCharacter) : base("Collect Resources", playerCharacter)
        {
            IsVisible = false;
            SetStateTransitionTarget("Return");
            AddOnExit(ReturnToGameScreen);
        }

        public void SetTargetRegion(Region targetRegion)
        {
            _targetRegion = targetRegion;
        }

        public override void Enter()
        {
            MenuStateMachine.States.NavigateToState("Inventory Menu");
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = GetCharacter();
            InventoryTransferManager.Instance().ShowInventories(GetCharacter().Inventory(), _targetRegion, () => GetCharacter().States.NavigateToState("Return"));
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