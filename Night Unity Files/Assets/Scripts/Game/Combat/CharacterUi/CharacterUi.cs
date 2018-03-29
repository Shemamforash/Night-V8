using Facilitating.UIControllers;
using Game.Characters;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.CharacterUi
{
    public class CharacterUi : MonoBehaviour
    {
        protected UIHealthBarController _healthBarController;
        protected UIArmourController _armourController;

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