using Game.Characters;
using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Waiting : CombatState
    {
        public Waiting(CombatStateMachine parentMachine) : base(nameof(Waiting), parentMachine)
        {
        }

//        public override void Enter()
//        {
//            base.Enter();
//            if (!Weapon().Cocked) NavigateToState(nameof(Cocking));
//            if (Weapon().GetRemainingAmmo() != 0) return;
//            if (!(Character() is Player)) return;
//            CombatManager.CombatUi.EmptyMagazine();
//            CombatManager.CombatUi.SetMagazineText("NO AMMO");
//        }
//
//        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
//        {
//            base.OnInputDown(axis, isHeld, direction);
//            switch (axis)
//            {
//                case InputAxis.CancelCover:
//                    CombatManager.TakeCover(Character());
//                    break;
//                case InputAxis.Fire:
//                    NavigateToState(nameof(Firing));
//                    break;
//                case InputAxis.Reload:
//                    NavigateToState(nameof(Reloading));
//                    break;
//                case InputAxis.Horizontal:
//                    NavigateToState(direction > 0 ? nameof(Approaching) : nameof(Retreating));
//                    break;
//                case InputAxis.Flank:
//                    NavigateToState(nameof(Flanking));
//                    break;
//            }
//        }
    }
}