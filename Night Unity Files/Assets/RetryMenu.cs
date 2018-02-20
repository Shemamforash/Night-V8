using Game.Characters.Player;
using Game.Combat;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;

public class RetryMenu : Menu
{
	private static Player _player;
	private static int _currentDanger = 1;
	private static RetryMenu _instance;
	private static InventoryResourceType _ammoType;
	private int _currentSize = 1;

	public void Awake()
	{
		_instance = this;
	}
	
	public void NextScenario()
	{
		++_currentDanger;
		if(_currentDanger == 20) CombatManager.FailCombat();
		if (_currentDanger % 4 == 0)
		{
			++_currentSize;
			_player.Attributes.Strength.Increment();
		}
		CombatScenario scenario = CombatScenario.Generate(_currentDanger, _currentSize);
		CombatManager.EnterCombat(_player, scenario);
		_player.Weapon.IncreaseDurability();
		ResetPlayerAmmo();
		_player.Weapon.Reload(_player.Inventory());
	}

	private void ResetPlayerAmmo()
	{
		int magsRemaining = (int) _player.Inventory().GetResourceQuantity(_ammoType);
		int neededMags = 41 - magsRemaining;
		_player.Inventory().IncrementResource(_ammoType, neededMags);
	}
	
	public static void StartCombat(Player p, Weapon w)
	{
		_currentDanger = 0;
		p.EquipWeapon(w);
		_player = p;
		_ammoType = _player.Weapon.WeaponAttributes.AmmoType;
		_instance.NextScenario();
	}
}
