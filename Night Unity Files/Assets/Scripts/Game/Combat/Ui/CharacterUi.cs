using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class CharacterUi : MonoBehaviour
    {
        private UIArmourController _armourController;
        private UIHealthBarController _healthBarController;
        private CanvasGroup CanvasGroup;
        protected CanTakeDamage Character;
        private CanTakeDamage _lastCharacter;

        public virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            _healthBarController = gameObject.FindChildWithName<UIHealthBarController>("Health");
            _armourController = gameObject.FindChildWithName<UIArmourController>("Armour");
        }

        private void SetAlpha(float a)
        {
            CanvasGroup.alpha = a;
        }

        protected virtual void LateUpdate()
        {
            if (Character == null)
            {
                SetAlpha(0);
                return;
            }

            SetAlpha(1);
            _healthBarController.SetValue(Character.HealthController.GetHealth(), Character == _lastCharacter);
            _armourController.UpdateArmour(Character.ArmourController);
            _healthBarController.SetSicknessLevel(Character.GetSicknessLevel());
            _lastCharacter = Character;
        }

        public RectTransform ArmourRect() => _armourController.GetComponent<RectTransform>();
        public RectTransform HealthRect() => _healthBarController.GetComponent<RectTransform>();
    }
}