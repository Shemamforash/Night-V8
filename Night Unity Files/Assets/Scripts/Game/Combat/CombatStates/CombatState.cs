using Characters;
using Game.Combat.Weapons;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.StateMachines;
using Character = Game.Characters.Character;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected bool IsPlayerState;

        protected CombatState(string name, CombatManager parentCombatManager, bool isPlayerState) : base(name, parentCombatManager)
        {
            IsPlayerState = isPlayerState;
        }

        protected Character Character()
        {
            return ((CombatManager)ParentMachine).Character();
        }

        protected Weapon Weapon()
        {
            return Character().GetWeapon();
        }
    }
}