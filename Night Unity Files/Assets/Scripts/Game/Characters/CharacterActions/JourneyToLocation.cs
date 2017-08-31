using Characters;
using Game.World;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class JourneyToLocation : BaseCharacterAction
    {
        private Region _targetRegion;
        private InventoryManager _inventoryManager;
        
        public JourneyToLocation(Character character) : base("Journey To Location", character)
        {
            IsVisible = false;
            _inventoryManager = GameObject.Find("Pick Up Menu").GetComponent<InventoryManager>();
        }

        public void SetTargetRegion(Region targetRegion)
        {
            _targetRegion = targetRegion;
            IncreaseDuration(_targetRegion.Distance());
            Start();
        }

        public override void UpdateAction()
        {
            if (Ready && _timeRemaining > 0)
            {
                --_timeRemaining.Val;
                TryUpdateCallback();
                if (_timeRemaining == 0)
                {
                    DoAtLocation();
                }
            }
        }

        private void DoAtLocation()
        {
            MenuStateMachine.Instance.NavigateToState("Pick Up Menu");
            _inventoryManager.SetInventorys(Character.CharacterInventory, _targetRegion);
        }
        
        public override void Exit()
        {
            //TODO show resources
            Ready = false;
            ParentMachine.NavigateToState("Journey From Location");
            BaseCharacterAction journeyFrom = (BaseCharacterAction) ParentMachine.GetCurrentState();
            journeyFrom.IncreaseDuration(_targetRegion.Distance());
            journeyFrom.Start();
        }
    }
}