using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class WeaponDetailController : MonoBehaviour
{
    private EnhancedText _nameText, _inscriptionNameText, _inscriptionEffectText, _typeText;
    private EnhancedText _damageText, _fireRateText, _reloadSpeedText, _accuracyText, _handlingText, _dpsText, _capacityText;

    private Weapon _weapon;
    private DurabilityBarController _durabilityBar;

    public void Awake()
    {
        _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
        _typeText = gameObject.FindChildWithName<EnhancedText>("Type");
        _damageText = gameObject.FindChildWithName<EnhancedText>("Damage");
        _fireRateText = gameObject.FindChildWithName<EnhancedText>("Fire Rate");
        _dpsText = gameObject.FindChildWithName<EnhancedText>("DPS");
        _capacityText = gameObject.FindChildWithName<EnhancedText>("Capacity");
        _reloadSpeedText = gameObject.FindChildWithName<EnhancedText>("Reload Speed");
        _accuracyText = gameObject.FindChildWithName<EnhancedText>("Critical Chance");
        _handlingText = gameObject.FindChildWithName<EnhancedText>("Handling");
        GameObject inscriptionObject = gameObject.FindChildWithName("Inscription");
        _inscriptionNameText = inscriptionObject.FindChildWithName<EnhancedText>("Inscription Name");
        _inscriptionEffectText = inscriptionObject.FindChildWithName<EnhancedText>("Effect");
        _durabilityBar = gameObject.FindChildWithName<DurabilityBarController>("Durability Bar");
    }

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;
        UpdateWeaponInfo();
    }

    private void SetWeaponInfo(Weapon weapon)
    {
        WeaponAttributes attr = weapon.WeaponAttributes;
        _nameText.SetText(weapon.GetDisplayName());
        _dpsText.SetText(attr.DPS().Round(1).ToString());
        _damageText.SetText(attr.Val(AttributeType.Damage).Round(1) + " Damage");
        _fireRateText.SetText(attr.Val(AttributeType.FireRate).Round(1) + " Rounds/Sec");
        _reloadSpeedText.SetText(attr.Val(AttributeType.ReloadSpeed).Round(1) + "s Reload");
        _handlingText.SetText(attr.Val(AttributeType.Handling).Round(1) + "% Handling");
        _capacityText.SetText(Mathf.FloorToInt(attr.Val(AttributeType.Capacity)) + " Capacity");
        _accuracyText.SetText((attr.Val(AttributeType.Accuracy) * 100).Round(1) + "% Accuracy");
        Inscription inscription = weapon.GetInscription();
        string inscriptionText = inscription == null ? "No Inscription" : inscription.Name;
        _inscriptionNameText.SetText(inscriptionText);
        _inscriptionEffectText.SetText(inscription == null ? "-" : inscription.GetSummary());
        _typeText.SetText(weapon.WeaponType().ToString());
    }

    public void UpdateWeaponInfo()
    {
        if (_weapon == null) SetNoWeaponInfo();
        else SetWeaponInfo(_weapon);
        UpdateDurabilityParticles();
    }

    private void SetNoWeaponInfo()
    {
        _durabilityBar.Reset();
        _nameText.SetText("");
        _damageText.SetText("");
        _fireRateText.SetText("");
        _dpsText.SetText("Nothing Equipped");
        _capacityText.SetText("");
        _reloadSpeedText.SetText("");
        _accuracyText.SetText("");
        _handlingText.SetText("");
        _inscriptionNameText.SetText("");
        _inscriptionEffectText.SetText("");
        _typeText.SetText("");
    }

    public void Hide()
    {
        _durabilityBar.SetWeapon(null);
    }

    private void UpdateDurabilityParticles()
    {
        _durabilityBar.SetWeapon(_weapon);
    }

    public void CompareTo(Weapon weapon)
    {
        _damageText.SetText(GetAttributePrefix(weapon, AttributeType.Damage, " Damage"));
        _fireRateText.SetText(GetAttributePrefix(weapon, AttributeType.FireRate, " Fire Rate"));
        _accuracyText.SetText(GetAttributePrefix(weapon, AttributeType.Accuracy, "% Accuracy"));
        _reloadSpeedText.SetText(GetAttributePrefix(weapon, AttributeType.ReloadSpeed, "s Reload "));
        _handlingText.SetText(GetAttributePrefix(weapon, AttributeType.Handling, "% Handling"));
        _capacityText.SetText(GetAttributePrefix(weapon, AttributeType.Capacity, " Capacity"));
    }

    private string GetAttributePrefix(Weapon equipped, AttributeType attribute, string displayText)
    {
        float equippedValue = equipped.GetAttributeValue(attribute);
        float compareValue = _weapon.GetAttributeValue(attribute);

        if (attribute == AttributeType.Accuracy)
        {
            equippedValue *= 100;
            compareValue *= 100;
        }

        if (attribute == AttributeType.Capacity)
        {
            equippedValue = Mathf.FloorToInt(equippedValue);
            compareValue = Mathf.FloorToInt(compareValue);
        }
        else
        {
            equippedValue = equippedValue.Round(1);
            compareValue = compareValue.Round(1);
        }

        float difference = compareValue - equippedValue;
        difference = difference.Round(1);
        string differenceString = difference < 0 ? "-" + difference : "+" + difference;

        string prefixString = compareValue + displayText + " (" + differenceString + ")";
        return prefixString;
    }
}