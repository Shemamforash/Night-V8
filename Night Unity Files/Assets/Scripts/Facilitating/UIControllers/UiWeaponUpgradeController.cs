using Facilitating.UI.Elements;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
	public class UiWeaponUpgradeController : Menu
	{
		private EnhancedText _titleText;
		private static EnhancedText _upgradeText;
		private static Weapon _weapon;
		private static string _previousMenu;
	
		private static TextMeshProUGUI _leftText,
			_rightText,
			_centralText,
			_dpsText,
			_subTitleText,
			_modifierColumnOneText,
			_modifierColumn2Text,
			_topLeftAttributeText,
			_topRightAttributeText,
			_centreLeftAttributeText,
			_centreRightAttributeText,
			_bottomLeftAttributeText,
			_bottomRightAttributeText;

		private static bool _upgradingAllowed;

		public void Awake () {
			Helper.FindChildWithName<EnhancedButton>(gameObject, "Cancel")?.AddOnClick(ReturnToPreviousMenu);
			_upgradeText = Helper.FindChildWithName<Button>(gameObject, "Upgrade").transform.Find("Text").GetComponent<EnhancedText>();
			_rightText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Right Text");
			_leftText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Left Text");
			_centralText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Centre Text");
			_dpsText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "DPS");
			_subTitleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Capacity");
			_modifierColumn2Text = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Modifier 1");
			_modifierColumnOneText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Modifier 2");
			_topLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Damage");
			_topRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Range");
			_centreLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Fire Rate");
			_centreRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Critical Chance");
			_bottomLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Reload Speed");
			_bottomRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Handling");
		}
	
		private void ReturnToPreviousMenu()
		{
			MenuStateMachine.ShowMenu(_previousMenu);
		}

		public static void Show(Weapon weapon)
		{
			_weapon = weapon;
			_previousMenu = MenuStateMachine.States.GetCurrentState().Name;
			MenuStateMachine.ShowMenu("Weapon Upgrade Menu");
			SetWeaponText();
		}
	
		private static void SetWeaponText()
		{
			_centralText.text = _weapon.ExtendedName();
			_leftText.text = _weapon.GetWeaponType();
			_rightText.text = _weapon.Weight + "kg";
			_dpsText.text = Helper.Round(_weapon.WeaponAttributes.DPS(), 1) + "DPS";
			_subTitleText.text = "Magazine " + _weapon.GetRemainingAmmo() + "/" + (int)_weapon.WeaponAttributes.Capacity.CurrentValue();
			_modifierColumnOneText.text = _weapon.WeaponAttributes.WeaponClassDescription;
			_modifierColumn2Text.text = _weapon.WeaponAttributes.ModifierDescription;
			_topLeftAttributeText.text = _weapon.GetAttributeValue(AttributeType.Damage) + "DMG";
			_topRightAttributeText.text = _weapon.GetAttributeValue(AttributeType.Range) + "ACC";
			_centreLeftAttributeText.text = Helper.Round(_weapon.GetAttributeValue(AttributeType.FireRate), 1) + "ROF";
			_centreRightAttributeText.text = _weapon.GetAttributeValue(AttributeType.CriticalChance) + "CRIT";
			_bottomLeftAttributeText.text = Helper.Round(_weapon.GetAttributeValue(AttributeType.ReloadSpeed), 1) + "RLD";
			int upgradeCost = _weapon.GetUpgradeCost();
			if (upgradeCost <= WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Quantity())
			{
				_upgradeText.Text("Upgrade (" + _weapon.GetUpgradeCost() + ")");
				_upgradingAllowed = true;
			}
			else
			{
				_upgradeText.Text("Need " + upgradeCost + " to upgrade.");
				_upgradingAllowed = false;
			}
		}

		public void UpgradeWeapon()
		{
			if (!_upgradingAllowed) return;
			_weapon.IncreaseDurability();
			SetWeaponText();
		}

		public void EquipWeapon()
		{
			UIGearEquipController.DisplayCharacters(_weapon);
		}
	}
}
