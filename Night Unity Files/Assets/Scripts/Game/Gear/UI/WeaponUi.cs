using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponUi : InventoryUi
    {
        private GameObject _detailedSection;
        private TextMeshProUGUI _dpsText, _capacityText, _subClassText, _modifierText, _damageText, _accuracyText, _fireRateText, _criticalChanceText, _reloadSpeedText, _handlingText;

        public WeaponUi(Weapon linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
            SetLeftTextCallback(linkedObject.GetWeaponType);
            SetCentralTextCallback(linkedObject.ExtendedName);
            SetRightTextCallback(() => linkedObject.Weight + "kg");
            OnEnter(() => _detailedSection.SetActive(true));
            OnExit(() => _detailedSection.SetActive(false));
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            _detailedSection = Helper.FindChildWithName(GameObject, "Detailed").gameObject;
            _dpsText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "DPS");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Capacity");
            _modifierText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Modifier 1");
            _subClassText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Modifier 2");
            _damageText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Damage");
            _accuracyText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Accuracy");
            _fireRateText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Fire Rate");
            _criticalChanceText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Critical Chance");
            _reloadSpeedText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Reload Speed");
            _handlingText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Handling");
        }

        public override void Update()
        {
            base.Update();
            Weapon weapon = (Weapon)LinkedObject;
            _dpsText.text = Helper.Round(weapon.WeaponAttributes.DPS(), 1) + "DPS";
            _capacityText.text = "Magazine " + weapon.AmmoInMagazine.Val + "/" + weapon.Capacity;
            _subClassText.text = weapon.SubClass.GetDescription();
            _modifierText.text = weapon.SecondaryModifier.GetDescription(true);
            _damageText.text = weapon.GetAttributeValue(AttributeType.Damage) + "DMG";
            _accuracyText.text = weapon.GetAttributeValue(AttributeType.Accuracy) + "ACC";
            _fireRateText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.FireRate), 1) + "ROF";
            _criticalChanceText.text = weapon.GetAttributeValue(AttributeType.Handling) + "CRIT";
            _reloadSpeedText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.ReloadSpeed), 1) + "RLD";
            _handlingText.text = weapon.GetAttributeValue(AttributeType.CriticalChance) + "HDL";
        }
    }
}