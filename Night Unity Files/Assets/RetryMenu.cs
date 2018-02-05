using Game.Characters.Player;
using Game.Combat;
using UnityEngine;

public class RetryMenu : MonoBehaviour
{
	private Player player;
	private int currentDanger = 1;
	
	public void RestartScenario()
	{
		CombatManager.EnterCombat(CombatManager.Player, CombatManager.CurrentScenario);
	}

	public void NextScenario()
	{
		++currentDanger;
		CombatScenario scenario = CombatScenario.Generate(currentDanger);
		CombatManager.EnterCombat(player, scenario);
	}
}
