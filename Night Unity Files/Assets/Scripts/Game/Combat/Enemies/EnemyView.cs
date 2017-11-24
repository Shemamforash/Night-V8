using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;

namespace Game.Combat.Enemies
{
    public class EnemyView : ViewParent
    {
        public TextMeshProUGUI CoverText, DistanceText, VisionText, ActionText;//StrengthText, ArmourText, 
        private TextMeshProUGUI _nameText, _typeText;
        private UIArmourController _uiArmourController;
        private UIHealthBarController _lowerUiHealthBarController;
        private GameObject _targetObject, _alertedObject, _detectedObject;
        public UIAimController UiAimController;
        private GameObject _damageTextObject;
        
        public EnemyView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/EnemyItem") : base(linkedObject, parent, prefabLocation)
        {
            GameObject.SetActive(true);
            GameObject.GetComponent<EnhancedButton>().UseGlobalColours = false;
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            CoverText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Cover");
            DistanceText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Distance");
            VisionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Vision");
//            StrengthText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Strength Remaining");
//            ArmourText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Armour");
            UiAimController = Helper.FindChildWithName<UIAimController>(GameObject, "Aim Timer");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            _uiArmourController = Helper.FindChildWithName<UIArmourController>(GameObject, "Armour Bar");
            _lowerUiHealthBarController = Helper.FindChildWithName<UIHealthBarController>(GameObject, "Health Bar Bottom");
            _alertedObject = GameObject.Find("Alert");
            _detectedObject = GameObject.Find("Detected");
            _targetObject = GameObject.Find("Target");
            _targetObject.SetActive(false);
            _alertedObject.SetActive(false);
            _detectedObject.SetActive(false);
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
            _damageTextObject = Resources.Load<GameObject>("Prefabs/Damage Text");
        }

        public void SpawnDamageText(int value, bool critical)
        {
            GameObject damageText = Helper.InstantiateUiObject(_damageTextObject, GameObject.transform);
            damageText.GetComponent<UINumberPopupController>().ShowValue(value, critical);
        }

        public void SetHealth(float normalisedHealth)
        {
            _lowerUiHealthBarController.SetValue(normalisedHealth);
        }

        public void SetArmour(int armourLevel)
        {
            _uiArmourController.SetArmourValue(armourLevel);
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
            _uiArmourController.SetColor(color);
            _lowerUiHealthBarController.SetColor(color);
        }

        public void MarkUnselected()
        {
            _lowerUiHealthBarController.SetParticleEmissionOverDistance();
            _targetObject.SetActive(false);
        }

        public void MarkSelected()
        {
            _targetObject.SetActive(true);
        }

        public void MarkDead()
        {
            CoverText.text = "";
            DistanceText.text = "";
            VisionText.text = "";
//            StrengthText.text = "";
//            ArmourText.text = "";
            _nameText.text = _nameText.text + " DEAD";
            _typeText.text = "Corpse";
            ActionText.text = "";
            SetNavigatable(false);
        }

        public void SetUnaware()
        {
            _alertedObject.SetActive(false);
            _detectedObject.SetActive(false);
        }

        public void SetAlert()
        {
            _alertedObject.SetActive(true);
            _detectedObject.SetActive(false);
        }

        public void SetDetected()
        {
            _alertedObject.SetActive(false);
            _detectedObject.SetActive(true);
        }
    }
}