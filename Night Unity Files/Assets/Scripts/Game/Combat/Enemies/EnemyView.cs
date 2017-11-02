using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Enemies
{
    public class EnemyView : ViewParent
    {
        public TextMeshProUGUI CoverText, DistanceText, VisionText, StrengthText, ArmourText, AlertText, ActionText;
        private TextMeshProUGUI _nameText, _typeText;
        private HealthBarController _upperHealthBarController, _lowerHealthBarController;
        
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
            _upperHealthBarController = Helper.FindChildWithName<HealthBarController>(GameObject, "Health Bar Top");
            _lowerHealthBarController = Helper.FindChildWithName<HealthBarController>(GameObject, "Health Bar Bottom");
            AlertText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Alert");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
        }

        public void SetHealth(float normalisedHealth)
        {
            _upperHealthBarController.SetValue(normalisedHealth);
            _lowerHealthBarController.SetValue(normalisedHealth);
        }
        
        public override void Update()
        {
            base.Update();
            _nameText.text = LinkedObject.Name;
            _typeText.text = ((Enemy) LinkedObject).EnemyType();
        }

        public void SetColour(Color color)
        {
            GetGameObject().GetComponent<EnhancedButton>().SetColor(color);
            _upperHealthBarController.SetColor(color);
            _lowerHealthBarController.SetColor(color);
        }

        public void MarkUnselected()
        {
            _upperHealthBarController.SetParticleEmissionOverDistance();
            _lowerHealthBarController.SetParticleEmissionOverDistance();
        }
    }
}