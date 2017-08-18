using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Flanking : CombatState {
        public Flanking(CombatManager parentMachine, bool isPlayerState) : base("Flanking", parentMachine, isPlayerState)
        {
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Horizontal)
            {
                ParentCombatManager.ReturnToDefault();
            }
        }
    }
}
