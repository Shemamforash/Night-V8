using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Gear.UI
{
    public class GearUi : InventoryUi
    {
        private GameObject _detailedSection;
        protected GameObject _summarySection, _modifierSection, _statsSection;

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
            SetLeftTextCallback(linkedObject.Type.ToString);
            SetCentralTextCallback(linkedObject.ExtendedName);
            SetRightTextCallback(() => linkedObject.Weight + "kg");
            OnEnter(() => _detailedSection.SetActive(true));
            OnExit(() => _detailedSection.SetActive(false));
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            _detailedSection = Helper.FindChildWithName(GameObject, "Detailed").gameObject;
            _summarySection = Helper.FindChildWithName(_detailedSection, "Summary").gameObject;
            _modifierSection = Helper.FindChildWithName(_detailedSection, "Modifiers").gameObject;
            _statsSection = Helper.FindChildWithName(_detailedSection, "Stats").gameObject;
            TitleText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "DPS");
            SubTitleText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Capacity");
            ModifierColumn2Text = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Modifier 1");
            ModifierColumnOneText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Modifier 2");
            TopLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Damage");
            TopRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Accuracy");
            CentreLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Fire Rate");
            CentreRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Critical Chance");
            BottomLeftAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Reload Speed");
            BottomRightAttributeText = Helper.FindChildWithName<TextMeshProUGUI>(_detailedSection, "Handling");
        }
    }
}