using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class CharacterUi : MonoBehaviour
    {
        protected UIArmourController _armourController;
        protected UIHealthBarController _healthBarController;
        private CanvasGroup _canvasGroup;

        public virtual void Awake()
        {
            _armourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            _healthBarController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetAlpha(float a)
        {
            _canvasGroup.alpha = a;
        }
        
        public virtual UIHealthBarController GetHealthController(CharacterCombat enemy)
        {
            return _healthBarController;
        }

        public virtual UIArmourController GetArmourController(Character character)
        {
            return _armourController;
        }
    }
}