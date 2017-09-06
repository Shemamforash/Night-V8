using System;
using Characters;
using Game.Combat;
using Game.World;
using Game.World.Region;
using Game.World.Time;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private Action _targetReachedAction;

        public Travel(Character character) : base("Travel", character)
        {
            IsVisible = false;
            HourCallback = Character.Travel;
        }

        public void SetTargetRegion(Region targetRegion)
        {
            Character.CurrentRegion = targetRegion;
            IncreaseDuration(Character.CurrentRegion.Distance());
            Start();
        }

        public Region GetTargetRegion()
        {
            return Character.CurrentRegion;
        }

        public void SetTargetReachedAction(Action targetReachedAction)
        {
            _targetReachedAction = targetReachedAction;
        }

        public override void Exit()
        {
            CombatScenario scenario = Character.CurrentRegion.GetCombatScenario();
            if (scenario != null)
            {
                ParentMachine.NavigateToState("Combat");
            }
            else
            {
                ParentMachine.NavigateToState("Collect Resources");
            }
            
            _targetReachedAction();
        }
    }
}