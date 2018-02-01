using Game.Combat;
using UnityEngine;

public class RetryMenu : MonoBehaviour {

	public void RestartScenario()
	{
		CombatManager.EnterCombat(CombatManager.Player, CombatManager.CurrentScenario);
	}
}
