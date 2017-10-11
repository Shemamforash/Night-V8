using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.UI
{
    public class ArmourUi : GearUi
    {
        public ArmourUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
        }

        public override void Update()
        {
            base.Update();
            Armour.Armour armour = (Armour.Armour)LinkedObject;
            TitleText.text = armour.ArmourRating + " Armour";
            SubTitleText.text = "-" + armour.ArmourRating / 100 + "% damage";
            _modifierSection.SetActive(false);

            TopLeftAttributeText.text = "+" + armour.IntelligenceModifier + "INT";
            TopRightAttributeText.text = "+" + armour.StabilityModifier + "STAB";
            CentreLeftAttributeText.text = "+" + armour.StrengthModifier + "STR";
            CentreRightAttributeText.text = "+" + armour.EnduranceModifier + "END";
            BottomLeftAttributeText.gameObject.SetActive(false);
            BottomRightAttributeText.gameObject.SetActive(false);
        }
    }
}