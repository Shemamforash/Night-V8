using Game.Combat.Weapons;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.MenuSystem;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected readonly bool IsPlayerState;

        protected CombatState(string name, CombatStateMachine parentCombatManager, bool isPlayerState) : base(name, parentCombatManager)
        {
            IsPlayerState = isPlayerState;
        }

        protected static Character Character()
        {
            return CombatManager.Instance().Character();
        }

        protected static Weapon Weapon()
        {
            return Character().GetWeapon();
        }
    }
}