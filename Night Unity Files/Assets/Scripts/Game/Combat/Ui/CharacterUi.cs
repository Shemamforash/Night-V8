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

        public virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        private void SetAlpha(float a)
        {
            CanvasGroup.alpha = a;
        }

        protected UIHealthBarController GetHealthController()
        {
            if (_healthBarController == null) _healthBarController = gameObject.FindChildWithName<UIHealthBarController>("Health");
            return _healthBarController;
        }

        protected UIArmourController GetArmourController()
        {
            if (_armourController == null) _armourController = gameObject.FindChildWithName<UIArmourController>("Armour");
            return _armourController;
        }

        protected virtual void LateUpdate()
        {
            if (Character == null)
            {
                SetAlpha(0);
                return;
            }

            SetAlpha(1);
            if (Character.IsBurning()) GetHealthController().StartBurning();
            else GetHealthController().StopBurning();
            GetHealthController().SetSicknessLevel(Character.GetSicknessLevel());
            GetArmourController().TakeDamage(Character.ArmourController);
        }
    }
}