using System;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected readonly CombatStateMachine CombatMachine;
        protected readonly bool IsPlayer;

        protected CombatState(string name, CombatStateMachine combatMachine) : base(name, StateSubtype.Combat)
        {
            CombatMachine = combatMachine;
            IsPlayer = CombatMachine.Character is Player;
        }

        public override void Enter()
        {
            if (!IsPlayer) return;
            Debug.Log(Name);
        }
        
        protected Character Character()
        {
            return CombatMachine.Character;
        }

        protected override void ReturnToDefault()
        {
            CombatMachine.ReturnToDefault();
        }

        protected override void NavigateToState(string name)
        {
            CombatMachine.NavigateToState(name);
        }

        protected Weapon Weapon()
        {
            return (Weapon)Character().GetGearItem(GearSubtype.Weapon);
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis != InputAxis.Submit) return;
            CombatManager.TryStartRageMode();
        }

        public override void OnInputUp(InputAxis axis)
        {
            if(axis == InputAxis.Sprint) Character().StopSprinting();
        }
    }
}