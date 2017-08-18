using Characters;
using SamsHelper.BaseGameFunctionality;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected CombatManager ParentCombatManager;
        protected bool IsPlayerState;

        protected CombatState(string name, CombatManager parentCombatManager, bool isPlayerState) : base(name, parentCombatManager)
        {
            ParentCombatManager = parentCombatManager;
            IsPlayerState = isPlayerState;
        }

        protected Character Character()
        {
            return ParentCombatManager.Character();
        }

        protected Weapon Weapon()
        {
            return Character().GetWeapon();
        }
    }
}