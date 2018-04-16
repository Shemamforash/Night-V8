using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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

        public void Update()
        {
        }
    }
}