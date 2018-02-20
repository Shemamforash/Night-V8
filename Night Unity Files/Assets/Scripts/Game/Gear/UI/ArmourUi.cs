using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gear.UI
{
    using Armour;

    public class ArmourUi : GearUi
    {
        public ArmourUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
            DetailedSection.GetComponent<LayoutElement>().minHeight = 100;
        }

        public override void Update()
        {
            base.Update();
            ArmourPlate armour = (ArmourPlate) LinkedObject;
            TitleText.text = armour.Weight + " Armour";
           //TODO SubTitleText.text = "-" + armour.ArmourRating / 100 + "% damage";
            ModifierColumnOneText.text = armour.GetSummary();
            ModifierColumn2Text.gameObject.SetActive(false);

//            TopLeftAttributeText.text = "+" + armour.PerceptionModifier + "INT";
//            TopRightAttributeText.text = "+" + armour.WillpowerModifier + "STAB";
//            CentreLeftAttributeText.text = "+" + armour.StrengthModifier + "STR";
//            CentreRightAttributeText.text = "+" + armour.EnduranceModifier + "END";
            BottomLeftAttributeText.gameObject.SetActive(false);
            BottomRightAttributeText.gameObject.SetActive(false);
        }
    }
}