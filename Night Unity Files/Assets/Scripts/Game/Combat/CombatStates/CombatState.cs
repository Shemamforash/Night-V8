using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected readonly CombatStateMachine CombatMachine;

        protected CombatState(string name, CombatStateMachine combatMachine) : base(name, StateSubtype.Combat, combatMachine)
        {
            CombatMachine = combatMachine;
        }

        public override void Enter()
        {
            Debug.Log(Name);
        }
        
        protected Character Character()
        {
            return CombatMachine.Character;
        }

        protected Weapon Weapon()
        {
            return (Weapon)Character().GetGearItem(GearSubtype.Weapon);
        }
    }
}