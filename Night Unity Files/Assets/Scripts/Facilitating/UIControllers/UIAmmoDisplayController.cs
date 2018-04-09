using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIAmmoDisplayController : MonoBehaviour
    {
        private static UIAmmoDisplayController _instance;
        public EnhancedText PistolText, RifleText, ShotgunText, SMGText, LMGText;

        public void Awake()
        {
            _instance = this;
        }

        public static UIAmmoDisplayController Instance()
        {
            return _instance;
        }

        public void SetPistolText(string quantity)
        {
            PistolText.Text(quantity);
        }

        public void SetRifleText(string quantity)
        {
            RifleText.Text(quantity);
        }

        public void SetShotgunText(string quantity)
        {
            ShotgunText.Text(quantity);
        }

        public void SetSmgText(string quantity)
        {
            SMGText.Text(quantity);
        }

        public void SetLmgText(string quantity)
        {
            LMGText.Text(quantity);
        }
    }
}