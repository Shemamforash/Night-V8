using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Combat.CombatStates
{
    public class Aiming : CombatState
    {
        public Aiming(CombatStateMachine parentMachine, bool isPlayerState) : base("Aiming", parentMachine, isPlayerState)
        {
            OnUpdate += IncreaseAim;
        }

        public override void Enter()
        {
            if (Weapon().GetRemainingAmmo() != 0) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("NO AMMO");
        }
        
        private void IncreaseAim()
        {
            CombatMachine.IncreaseAim();
        }

        public override void OnInputDown(InputAxis inputAxis)
        {
            switch (inputAxis)
            {
                case InputAxis.Fire:
                    ParentMachine.NavigateToState("Firing");
                    break;
                case InputAxis.Reload:
                    ParentMachine.NavigateToState("Reloading");
                    break;
                case InputAxis.Vertical:
                    ParentMachine.NavigateToState(InputSpeaker.LastInputValue() > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Horizontal:
                    ParentMachine.NavigateToState(InputSpeaker.LastInputValue() > 0 ? "Flanking" : "EnteringCover");
                    break;
            }
        }
    }
}