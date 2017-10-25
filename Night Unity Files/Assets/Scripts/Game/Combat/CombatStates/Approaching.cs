using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Approaching : CombatState
    {
        public Approaching(CombatStateMachine parentMachine) : base("Approaching", parentMachine)
        {
            OnUpdate += () => CombatMachine.Character.DecreaseDistance();
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}