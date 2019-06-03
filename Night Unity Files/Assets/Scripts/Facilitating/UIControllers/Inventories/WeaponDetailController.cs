using System.Globalization;
using Extensions;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class WeaponDetailController : MonoBehaviour
{
	private EnhancedText _damageText,    _fireRateText, _reloadSpeedText, _accuracyText, _rangeText, _dpsText, _capacityText, _shatterText, _burnText, _voidText;
	private EnhancedText _nameText,      _typeText;
	private GameObject   _shatterObject, _burnObject, _voidObject;

	private                  Weapon _weapon;
	[SerializeField] private bool   IsDetailed;

	public void Awake()
	{
		_nameText = gameObject.FindChildWithName<EnhancedText>("Name");
		_typeText = gameObject.FindChildWithName<EnhancedText>("Type");
		_dpsText  = gameObject.FindChildWithName<EnhancedText>("DPS");

		if (!IsDetailed) return;
		_damageText      = gameObject.FindChildWithName<EnhancedText>("Damage");
		_fireRateText    = gameObject.FindChildWithName<EnhancedText>("Fire Rate");
		_capacityText    = gameObject.FindChildWithName<EnhancedText>("Capacity");
		_reloadSpeedText = gameObject.FindChildWithName<EnhancedText>("Reload Speed");
		_accuracyText    = gameObject.FindChildWithName<EnhancedText>("Critical Chance");
		_rangeText       = gameObject.FindChildWithName<EnhancedText>("Handling");
		GameObject conditionObject = gameObject.FindChildWithName("Conditions");

		_shatterObject = conditionObject.FindChildWithName("Shatter");
		_shatterText   = _shatterObject.FindChildWithName<EnhancedText>("Text");

		_burnObject = conditionObject.FindChildWithName("Burn");
		_burnText   = _burnObject.FindChildWithName<EnhancedText>("Text");

		_voidObject = conditionObject.FindChildWithName("Sickness");
		_voidText   = _voidObject.FindChildWithName<EnhancedText>("Text");
	}

	public void SetWeapon(Weapon weapon)
	{
		_weapon = weapon;
		SetWeaponInfo(_weapon);
	}

	private void SetWeaponInfo(Weapon weapon)
	{
		_nameText.SetText(weapon.Name);
		string dps = weapon.WeaponAttributes.DPS().Round(1).ToString(CultureInfo.InvariantCulture);
		dps += "\n<size=15><color=#aaaaaa>DPS</color></size>";
		_dpsText.SetText(dps);
		_typeText.SetText(weapon.WeaponAttributes.GetWeaponTypeDescription());
		SetConditionText();
		SetAttributeText(weapon);
	}

	private void SetAttributeText(Weapon weapon)
	{
		if (!IsDetailed) return;
		_damageText.SetText(weapon.WeaponAttributes.Val(AttributeType.Damage).Round(1)              + " Damage");
		_fireRateText.SetText(weapon.WeaponAttributes.Val(AttributeType.FireRate).Round(1)          + " Rounds/Sec");
		_reloadSpeedText.SetText(weapon.WeaponAttributes.Val(AttributeType.ReloadSpeed).Round(1)    + "s Reload");
		_rangeText.SetText(weapon.WeaponAttributes.CalculateRange().Round(1)                        + " Range");
		_capacityText.SetText(Mathf.FloorToInt(weapon.WeaponAttributes.Val(AttributeType.Capacity)) + " Capacity");
		_accuracyText.SetText((weapon.WeaponAttributes.Val(AttributeType.Accuracy) * 100).Round(1)  + "% Accuracy");
	}

	private void SetConditionText()
	{
		if (!IsDetailed) return;
		float shatterChance = _weapon.WeaponAttributes.CalculateShatterChance();
		float burnChance    = _weapon.WeaponAttributes.CalculateBurnChance();
		float voidChance    = _weapon.WeaponAttributes.CalculateVoidChance();
		Debug.Log(shatterChance);

		_shatterObject.SetActive(shatterChance != 0);
		_burnObject.SetActive(burnChance       != 0);
		_voidObject.SetActive(voidChance       != 0);
		_shatterText.SetText(shatterChance == 0 ? "" : shatterChance + " Shatter");
		_burnText.SetText(burnChance       == 0 ? "" : burnChance    + " Burn");
		_voidText.SetText(voidChance       == 0 ? "" : voidChance    + " Void");
	}
}