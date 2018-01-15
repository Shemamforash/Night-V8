using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class EnemyView : ViewParent
    {
        public TextMeshProUGUI CoverText, DistanceText, RangeText, ActionText, HealthText, ArmourText; 
        private TextMeshProUGUI _nameText, _typeText;
        private UIArmourController _uiArmourController;
        private UIHealthBarController _lowerUiHealthBarController;
        private GameObject _alertedObject, _detectedObject;
        public UIAimController UiAimController;
        private float _fadingIn = 2f;
        private float _fadeInTime = 2f;
        
        public EnemyView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/EnemyItem") : base(linkedObject, parent, prefabLocation)
        {
            GameObject.SetActive(true);
            SetAlpha(0f);
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            CoverText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Cover");
            CoverText.text = "No Cover";
            DistanceText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Distance");
            RangeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Range Category");
            HealthText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Health Text");
            ArmourText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Armour Text");
            UiAimController = Helper.FindChildWithName<UIAimController>(GameObject, "Aim Timer");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            _uiArmourController = Helper.FindChildWithName<UIArmourController>(GameObject, "Armour Bar");
            _lowerUiHealthBarController = Helper.FindChildWithName<UIHealthBarController>(GameObject, "Health Bar");
            _alertedObject = GameObject.Find("Alert");
            _detectedObject = GameObject.Find("Detected");
            _alertedObject.SetActive(false);
            _detectedObject.SetActive(false);
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
        }

        public void SetHealth(HealthController healthController)
        {
            _lowerUiHealthBarController.SetValue(healthController.GetNormalisedHealthValue());
            HealthText.text = (int)healthController.GetCurrentHealth() + "/" + (int)healthController.GetMaxHealth();
        }

        public void SetArmour(int armourLevel, bool inCover)
        {
            _uiArmourController.SetArmourValue(armourLevel);
            float armourProtection = 1 - armourLevel / 10f;
            string coverString = "No Cover";
            if (inCover)
            {
                armourProtection /= 2f;
                coverString = "In Cover";
            }
            CoverText.text = coverString;
            ArmourText.text = armourProtection + "x damage";
        }
        
        public override void Update()
        {
            base.Update();
            _nameText.text = LinkedObject.Name;
            _typeText.text = ((Enemy) LinkedObject).EnemyType();
        }

        public void SetAlpha(float alpha)
        {
            if (_fadingIn > 0)
            {
                float fadeInAmount = 1f - _fadingIn / _fadeInTime;
                alpha *= fadeInAmount;
                _fadingIn -= Time.deltaTime;
            }
            GetGameObject().GetComponent<CanvasGroup>().alpha = alpha;
        }

        public void MarkUnselected()
        {
            _lowerUiHealthBarController.SetParticleEmissionOverDistance();
        }

        public void MarkDead()
        {
//            CoverText.text = "";
            DistanceText.text = "";
            RangeText.text = "";
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