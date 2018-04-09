using Game.Exploration.Region;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    //todo move me somewhere better
    public class CollectResources : BaseCharacterAction
    {
        private Player _previousCharacter;
        private Region _targetRegion;

        public CollectResources(Player playerCharacter) : base("Collect Resources", playerCharacter)
        {
            IsVisible = false;
//            SetStateTransitionTarget(playerCharacter.ReturnAction);
            AddOnExit(ReturnToGameScreen);
        }

        public void SetTargetRegion(Region targetRegion)
        {
            _targetRegion = targetRegion;
            _previousCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectedCharacter = PlayerCharacter;
//            InventoryTransferManager.Instance().ShowInventories(PlayerCharacter.Inventory(), _targetRegion, () => PlayerCharacter.ReturnAction.Enter());
        }

//        public override void Interrupt()
//        {
//            ReturnToGameScreen();
//        }

//        public override void Resume()
//        {
//            Enter();
//        }

        private void ReturnToGameScreen()
        {
            CharacterManager.SelectedCharacter = _previousCharacter;
            MenuStateMachine.GoToInitialMenu();
        }
    }
}