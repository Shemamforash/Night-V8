using System.Globalization;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class WeaponDetailController : MonoBehaviour
{
    private EnhancedText _nameText, _inscriptionNameText, _inscriptionEffectText, _typeText;
    private EnhancedText _damageText, _fireRateText, _reloadSpeedText, _accuracyText, _recoilText, _dpsText, _capacityText, _shatterText, _burnText, _voidText;
    private GameObject _shatterObject, _burnObject, _voidObject;

    private Weapon _weapon;
    private DurabilityBarController _durabilityBar;
    [SerializeField] private bool IsDetailed;

    public void Awake()
    {
        _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
        _typeText = gameObject.FindChildWithName<EnhancedText>("Type");
        _dpsText = gameObject.FindChildWithName<EnhancedText>("DPS");
        _durabilityBar = gameObject.FindChildWithName<DurabilityBarController>("Durability Bar");

        if (!IsDetailed) return;
        _damageText = gameObject.FindChildWithName<EnhancedText>("Damage");
        _fireRateText = gameObject.FindChildWithName<EnhancedText>("Fire Rate");
        GameObject inscriptionObject = gameObject.FindChildWithName("Inscription");
        _inscriptionNameText = inscriptionObject.FindChildWithName<EnhancedText>("Inscription Name");
        _inscriptionEffectText = inscriptionObject.FindChildWithName<EnhancedText>("Effect");
        _capacityText = gameObject.FindChildWithName<EnhancedText>("Capacity");
        _reloadSpeedText = gameObject.FindChildWithName<EnhancedText>("Reload Speed");
        _accuracyText = gameObject.FindChildWithName<EnhancedText>("Critical Chance");
        _recoilText = gameObject.FindChildWithName<EnhancedText>("Handling");
        GameObject conditionObject = gameObject.FindChildWithName("Conditions");

        _shatterObject = conditionObject.FindChildWithName("Shatter");
        _shatterText = _shatterObject.FindChildWithName<EnhancedText>("Text");

        _burnObject = conditionObject.FindChildWithName("Burn");
        _burnText = _burnObject.FindChildWithName<EnhancedText>("Text");

        _voidObject = conditionObject.FindChildWithName("Sickness");
        _voidText = _voidObject.FindChildWithName<EnhancedText>("Text");
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
        _dpsText.SetText(attr.DPS().Round(1).ToString(CultureInfo.InvariantCulture));
        _typeText.SetText(weapon.WeaponAttributes.GetWeaponTypeDescription());
        SetInscriptionText(weapon);
        SetConditionText();
        SetAttributeText(attr);
    }

    private void SetInscriptionText(Weapon weapon)
    {
        Inscription inscription = weapon.GetInscription();
        string inscriptionText = inscription == null ? "No Inscription" : inscription.Name;
        string inscriptionEffectText = inscription == null ? "" : inscription.GetSummary();
        if (!IsDetailed)
        {
            _typeText.SetText(weapon.WeaponAttributes.GetWeaponTypeDescription() + " - " + inscriptionEffectText);
            return;
        }

        _inscriptionNameText.SetText(inscriptionText);
        _inscriptionEffectText.SetText(inscriptionEffectText == "" ? "-" : inscriptionEffectText);
    }

    private void SetAttributeText(WeaponAttributes attr)
    {
        if (!IsDetailed) return;
        _damageText.SetText(attr.Val(AttributeType.Damage).Round(1) + " Damage");
        _fireRateText.SetText(attr.Val(AttributeType.FireRate).Round(1) + " Rounds/Sec");
        _reloadSpeedText.SetText(attr.Val(AttributeType.ReloadSpeed).Round(1) + "s Reload");
        _recoilText.SetText(attr.Val(AttributeType.Recoil).Round(1) + "% Recoil");
        _capacityText.SetText(Mathf.FloorToInt(attr.Val(AttributeType.Capacity)) + " Capacity");
        _accuracyText.SetText((attr.Val(AttributeType.Accuracy) * 100).Round(1) + "% Accuracy");
    }

    private void SetNoAttributeText()
    {
        if (!IsDetailed) return;
        _damageText.SetText("");
        _fireRateText.SetText("");
        _capacityText.SetText("");
        _reloadSpeedText.SetText("");
        _accuracyText.SetText("");
        _recoilText.SetText("");
    }

    private void SetConditionText()
    {
        if (!IsDetailed) return;
        WeaponAttributes attributes = _weapon?.WeaponAttributes;
        float shatterChance = attributes.CalculateShatterChance();
        float burnChance = attributes.CalculateBurnChance();
        float voidChance = attributes.CalculateVoidChance();

        _shatterObject.SetActive(shatterChance != 0);
        _burnObject.SetActive(burnChance != 0);
        _voidObject.SetActive(voidChance != 0);
        _shatterText.SetText(shatterChance == 0 ? "" : shatterChance + " Shatter");
        _burnText.SetText(burnChance == 0 ? "" : burnChance + " Burn");
        _voidText.SetText(voidChance == 0 ? "" : voidChance + " Void");
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
        _dpsText.SetText("Nothing Equipped");
        _inscriptionNameText.SetText("");
        _inscriptionEffectText.SetText("");
        _typeText.SetText("");
        SetNoAttributeText();
        SetConditionText();
    }

    private void UpdateDurabilityParticles()
    {
        _durabilityBar.SetWeapon(_weapon);
    }

    public RectTransform DurabilityRect()
    {
        if (_durabilityBar == null) _durabilityBar = gameObject.FindChildWithName<DurabilityBarController>("Durability Bar");
        return _durabilityBar.GetComponent<RectTransform>();
    }
}