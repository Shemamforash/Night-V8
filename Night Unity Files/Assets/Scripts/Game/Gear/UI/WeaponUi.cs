using Facilitating.UIControllers;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.UI
{
    public class WeaponUi : GearUi
    {
        public WeaponUi(Weapon linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
            SetLeftTextCallback(linkedObject.GetWeaponType);
            PrimaryButton.AddOnClick(() => UiWeaponUpgradeController.Show(linkedObject));
        }

        public override void Update()
        {
            base.Update();
            Weapon weapon = (Weapon)LinkedObject;
            TitleText.text = Helper.Round(weapon.WeaponAttributes.DPS(), 1) + "DPS";
            SubTitleText.text = "Magazine " + weapon.GetRemainingAmmo() + "/" + (int)weapon.WeaponAttributes.Capacity.CurrentValue();
            ModifierColumnOneText.text = weapon.WeaponAttributes.SubClassDescription;
            ModifierColumn2Text.text = weapon.WeaponAttributes.ModifierDescription;
            TopLeftAttributeText.text = weapon.GetAttributeValue(AttributeType.Damage) + "DMG";
            TopRightAttributeText.text = weapon.GetAttributeValue(AttributeType.Accuracy) + "ACC";
            CentreLeftAttributeText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.FireRate), 1) + "ROF";
            CentreRightAttributeText.text = weapon.GetAttributeValue(AttributeType.Handling) + "CRIT";
            BottomLeftAttributeText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.ReloadSpeed), 1) + "RLD";
            BottomRightAttributeText.text = weapon.GetAttributeValue(AttributeType.CriticalChance) + "HDL";
        }
    }
}