using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIAttributeMarkerController : MonoBehaviour
    {
        private Image _active, _inactive, _overchargedActive, _overchargedInactive;

        public void Awake()
        {
            _active = Helper.FindChildWithName<Image>(gameObject, "Active");
            _inactive = Helper.FindChildWithName<Image>(gameObject, "Inactive");
            _overchargedActive = Helper.FindChildWithName<Image>(gameObject, "Active Second");
            _overchargedInactive = Helper.FindChildWithName<Image>(gameObject, "Inactive Second");
        }

        public void SetValue(CharacterAttribute attribute)
        {
            int currentValue = Mathf.CeilToInt(attribute.CurrentValue());
            int max = (int) attribute.Max;
            if (max > 10)
            {
                int overchargeMax = max - 10;
                _overchargedInactive.fillAmount = overchargeMax / 10f;
                _inactive.fillAmount = 1;
            }
            else
            {
                _overchargedInactive.fillAmount = 0f;
                _inactive.fillAmount = max / 10f;
            }

            if (currentValue > 10)
            {
                int overchargeCurrent = currentValue - 10;
                _overchargedActive.fillAmount = overchargeCurrent / 10f;
                _active.fillAmount = 1f;
            }
            else
            {
                _overchargedActive.fillAmount = 0f;
                _active.fillAmount = currentValue / 10f;
            }
        }
    }
}