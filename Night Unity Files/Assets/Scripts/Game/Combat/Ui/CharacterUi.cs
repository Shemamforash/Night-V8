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

        public virtual void Awake()
        {
            _armourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            _healthBarController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
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