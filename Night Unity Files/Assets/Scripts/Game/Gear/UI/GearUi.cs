using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Gear.UI
{
    public class GearUi : InventoryUi
    {
        protected GameObject DetailedSection;
        protected GameObject SummarySection, ModifierSection, StatsSection;

        protected TextMeshProUGUI TitleText,
            SubTitleText,
            ModifierColumnOneText,
            ModifierColumn2Text,
            TopLeftAttributeText,
            TopRightAttributeText,
            CentreLeftAttributeText,
            CentreRightAttributeText,
            BottomLeftAttributeText,
            BottomRightAttributeText;

        protected GearUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/FlexibleItem") : base(linkedObject, parent, prefabLocation)
        {
            GearItem gear = (GearItem) linkedObject;
            SetLeftTextCallback(gear.GetGearType().ToString);
            SetCentralTextCallback(linkedObject.ExtendedName);
            SetRightTextCallback(() => linkedObject.Weight + "kg");
            PrimaryButton.AddOnSelectEvent(() => DetailedSection.SetActive(true));
            PrimaryButton.AddOnDeselectEvent(() => DetailedSection.SetActive(false));
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            DetailedSection = Helper.FindChildWithName(GameObject, "Detailed").gameObject;
            DetailedSection.SetActive(false);
            SummarySection = Helper.FindChildWithName(DetailedSection, "Summary").gameObject;
            ModifierSection = Helper.FindChildWithName(DetailedSection, "Modifiers").gameObject;
            StatsSection = Helper.FindChildWithName(DetailedSection, "Stats").gameObject;
            TitleText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "DPS");
            SubTitleText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Capacity");
            ModifierColumn2Text = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Modifier 1");
            ModifierColumnOneText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Modifier 2");
            TopLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Damage");
            TopRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Accuracy");
            CentreLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Fire Rate");
            CentreRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Critical Chance");
            BottomLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Reload Speed");
            BottomRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(DetailedSection, "Handling");
        }
    }
}