using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Enemies
{
    public class EnemyView : ViewParent
    {
        public TextMeshProUGUI CoverText, DistanceText, VisionText, StrengthText, ArmourText, StatusEffects;
        private TextMeshProUGUI _nameText, _typeText;
        public Slider HealthSlider;
        
        public EnemyView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/EnemyItem") : base(linkedObject, parent, prefabLocation)
        {
            GameObject.SetActive(true);
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            CoverText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Cover");
            DistanceText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Distance");
            VisionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Vision");
            StrengthText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Strength Remaining");
            ArmourText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Armour");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            HealthSlider = Helper.FindChildWithName<Slider>(GameObject, "Health Bar");
            StatusEffects = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Status Effects");
        }

        public override void Update()
        {
            _nameText.text = LinkedObject.Name;
            _typeText.text = ((Enemy) LinkedObject).EnemyType();
        }
    }
}