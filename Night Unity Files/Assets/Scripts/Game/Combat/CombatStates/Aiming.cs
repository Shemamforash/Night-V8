using System;
using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Aiming : CombatState
    {
        public Aiming(CombatManager parentMachine, bool isPlayerState) : base("Aiming", parentMachine, isPlayerState)
        {
        }

        public override void Enter()
        {
            if (Weapon().GetRemainingAmmo() == 0)
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText("NO AMMO");
            }
        }
        
        public override void Update()
        {
            ParentCombatManager.IncreaseAim();
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