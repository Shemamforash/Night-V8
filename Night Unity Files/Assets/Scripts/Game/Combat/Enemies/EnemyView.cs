﻿using SamsHelper;
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
        private ArmourController _armourController;
        private HealthBarController _lowerHealthBarController;
        private GameObject _targetObject, _alertedObject, _detectedObject;
        private AimController _aimController;
        
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
            _aimController = Helper.FindChildWithName<AimController>(GameObject, "Aim Timer");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            _armourController = Helper.FindChildWithName<ArmourController>(GameObject, "Armour Bar");
            _lowerHealthBarController = Helper.FindChildWithName<HealthBarController>(GameObject, "Health Bar Bottom");
            _alertedObject = GameObject.Find("Alert");
            _detectedObject = GameObject.Find("Detected");
            _targetObject = GameObject.Find("Target");
            _targetObject.SetActive(false);
            _alertedObject.SetActive(false);
            _detectedObject.SetActive(false);
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
        }

        public void SetAimTimerValue(float value)
        {
            _aimController.SetValue(value);
        }

        public void SetHealth(float normalisedHealth)
        {
            _lowerHealthBarController.SetValue(normalisedHealth);
        }

        public void SetArmour(int armourLevel)
        {
            _armourController.SetArmourValue(armourLevel);
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
            _armourController.SetColor(color);
            _lowerHealthBarController.SetColor(color);
        }

        public void MarkUnselected()
        {
            _lowerHealthBarController.SetParticleEmissionOverDistance();
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