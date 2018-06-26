using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class CharacterUi : MonoBehaviour
    {
        public UIArmourController _armourController;
        protected UIHealthBarController _healthBarController;
        protected CanvasGroup CanvasGroup;

        public virtual void Awake()
        {
            _armourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            _healthBarController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        protected void SetAlpha(float a)
        {
            CanvasGroup.alpha = a;
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