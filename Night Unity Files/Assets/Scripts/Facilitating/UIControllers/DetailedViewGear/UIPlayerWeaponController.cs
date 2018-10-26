using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText;
        private GameObject _notEquippedObject, _equippedObject;
        public EnhancedButton EnhancedButton;
        private DurabilityBarController _durabilityBar;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _notEquippedObject = gameObject.FindChildWithName("Not Equipped");
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
            _durabilityBar = gameObject.FindChildWithName<DurabilityBarController>("Durability");
        }

        public void SetWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                _equippedObject.SetActive(false);
                _notEquippedObject.SetActive(true);
            }
            else
            {
                _notEquippedObject.SetActive(false);
                _equippedObject.SetActive(true);
                _nameText.SetText(weapon.Name);
            }
            _durabilityBar.SetWeapon(weapon);
        }
    }
}