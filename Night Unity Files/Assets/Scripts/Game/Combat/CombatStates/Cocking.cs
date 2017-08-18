using SamsHelper.BaseGameFunctionality;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
	public class Cocking : CombatState {
		public Cocking(CombatManager parentMachine, bool isPlayerState) : base("Cocking", parentMachine, isPlayerState)
		{
		}

		public override void Enter()
		{
			CombatManager.CombatUi.EmptyMagazine();
			CombatManager.CombatUi.SetMagazineText("EJECT CARTRIDGE");
		}
		
		public override void Exit()
		{
			CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
			ParentMachine.ReturnToDefault();
		}

		public override void OnInputDown(InputAxis inputAxis)
		{
			if (inputAxis == InputAxis.Cancel)
			{
				CombatManager.CombatUi.EmptyMagazine();
				new Cooldown(Weapon().FireRate, Exit, f => CombatManager.CombatUi.UpdateReloadTime(f));
			}
		}
	}
}
