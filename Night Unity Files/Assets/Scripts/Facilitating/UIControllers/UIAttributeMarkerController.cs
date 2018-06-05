using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIAttributeMarkerController : MonoBehaviour
    {
        private Image _active, _inactive;

        public void Awake()
        {
            _active = Helper.FindChildWithName<Image>(gameObject, "Active");
            _inactive = Helper.FindChildWithName<Image>(gameObject, "Inactive");
        }

        public void SetValue(CharacterAttribute attribute)
        {
            int currentValue = Mathf.CeilToInt(attribute.CurrentValue());
            int max = (int) attribute.Max;
            _inactive.fillAmount = max / 10f;
            _active.fillAmount = currentValue / 10f;
        }
    }
}