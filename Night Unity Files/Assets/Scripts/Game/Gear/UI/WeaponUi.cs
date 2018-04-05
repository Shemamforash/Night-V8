using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.UI
{
    public class WeaponUi : GearUi
    {
        public WeaponUi(Weapon linkedObject, Transform parent) : base(linkedObject, parent)
        {
            SetLeftTextCallback(linkedObject.GetWeaponType);
        }

        public override void Update()
        {
            base.Update();
            Weapon weapon = (Weapon)LinkedObject;
            TitleText.text = Helper.Round(weapon.WeaponAttributes.DPS(), 1) + "DPS";
            SubTitleText.text = "Magazine " + weapon.GetRemainingAmmo() + "/" + (int)weapon.WeaponAttributes.Capacity.CurrentValue();
            ModifierColumnOneText.text = weapon.WeaponAttributes.WeaponClassDescription;
            ModifierColumn2Text.text = weapon.WeaponAttributes.ModifierDescription;
            TopLeftAttributeText.text = weapon.GetAttributeValue(AttributeType.Damage) + "DMG";
            TopRightAttributeText.text = weapon.GetAttributeValue(AttributeType.Range) + "RAN";
            CentreLeftAttributeText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.FireRate), 1) + "ROF";
            BottomLeftAttributeText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.ReloadSpeed), 1) + "RLD";
            BottomRightAttributeText.text = Helper.Round(weapon.GetAttributeValue(AttributeType.Accuracy), 1) + "ACC";
        }
    }
}