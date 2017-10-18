using Game.Combat.Weapons;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.MenuSystem;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected readonly bool IsPlayerState;
        protected CombatStateMachine CombatMachine;

        protected CombatState(string name, CombatStateMachine combatMachine, bool isPlayerState) : base(name, StateSubtype.Combat, combatMachine)
        {
            CombatMachine = combatMachine;
            IsPlayerState = isPlayerState;
        }

        protected Character Character()
        {
            return CombatMachine.Character;
        }

        protected Weapon Weapon()
        {
            return (Weapon)Character().EquippedGear[GearSubtype.Weapon];
        }
    }
}