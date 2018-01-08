using Game.World.Region;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class CollectResources : BaseCharacterAction
    {
        private Player _previousCharacter;
        private Region _targetRegion;

        public CollectResources(Player playerCharacter) : base("Collect Resources", playerCharacter)
        {
            IsVisible = false;
            SetStateTransitionTarget(playerCharacter.ReturnAction);
            AddOnExit(ReturnToGameScreen);
        }

        public void SetTargetRegion(Region targetRegion)
        {
            _targetRegion = targetRegion;
            Enter();
        }

        public override void Enter()
        {
            base.Enter();
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = GetCharacter();
            InventoryTransferManager.Instance().ShowInventories(GetCharacter().Inventory(), _targetRegion, () => GetCharacter().ReturnAction.Enter());
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