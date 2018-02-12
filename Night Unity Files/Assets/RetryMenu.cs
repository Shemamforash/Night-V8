using Game.Characters.Player;
using Game.Combat;
using Game.Gear.Weapons;
using SamsHelper.ReactiveUI.MenuSystem;

public class RetryMenu : Menu
{
	private static Player _player;
	private static int _currentDanger = 1;
	private static RetryMenu _instance;

	public void Awake()
	{
		_instance = this;
	}
	
	public void NextScenario()
	{
		++_currentDanger;
		CombatScenario scenario = CombatScenario.Generate(_currentDanger);
		CombatManager.EnterCombat(_player, scenario);
		_player.Weapon.IncreaseDurability();
	}

	public static void StartCombat(Player p, Weapon w)
	{
		_currentDanger = 0;
		p.EquipWeapon(w);
		_player = p;
		_player.Inventory().IncrementResource(w.WeaponAttributes.AmmoType, 21);
		w.Reload(_player.Inventory());
		_instance.NextScenario();
	}
}
