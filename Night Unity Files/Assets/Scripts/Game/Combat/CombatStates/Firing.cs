using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Firing : CombatState
    {
        private float _timeSinceLastFire;
        
        public Firing(CombatManager parentMachine, bool isPlayerState) : base("Firing", parentMachine, isPlayerState)
        {
        }
        
        public override void Update()
        {
            if (Weapon().GetRemainingAmmo() > 0)
            {
                if (_timeSinceLastFire > 0)
                {
                    _timeSinceLastFire -= Time.deltaTime;
                }
                else
                {
                    Weapon().Fire();
                    if (IsPlayerState)
                    {
                        CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
                    }
                    ParentCombatManager.DecreaseAim();
                    if (Weapon().Automatic)
                    {
                        _timeSinceLastFire = 1f / Weapon().FireRate;
                    }
                    else
                    {
                        ParentCombatManager.NavigateToState("Cocking");
                    }
                }
            }
            else
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText("NO AMMO");
            }
        }
        
        public override void Enter()
        {
            _timeSinceLastFire = 0f;
        }

        public override void OnInputUp(InputAxis inputAxis)
        {
            if (inputAxis == InputAxis.Fire)
            {
                ParentCombatManager.NavigateToState("Aiming");
            }
        }
    }
}