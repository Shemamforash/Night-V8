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
        protected CanvasGroup CanvasGroup;

        public virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        protected void SetAlpha(float a)
        {
            CanvasGroup.alpha = a;
        }

        public virtual UIHealthBarController GetHealthController(CharacterCombat enemy)
        {
            if (_healthBarController == null) _healthBarController = gameObject.FindChildWithName<UIHealthBarController>("Health");
            return _healthBarController;
        }

        public virtual UIArmourController GetArmourController(Character character)
        {
            if (_armourController == null) _armourController = gameObject.FindChildWithName<UIArmourController>("Armour");
            return _armourController;
        }
    }
}