using DG.Tweening;
using Game.Gear.Armour;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIArmourController : MonoBehaviour
    {
        private Image _fillBar;
        private RectTransform _armourRect;
        private Image _working;
        private Tweener _fadeTween;

        public void Awake()
        {
            _armourRect = gameObject.FindChildWithName<RectTransform>("Bar");
            _fillBar = gameObject.FindChildWithName<Image>("Fill");
            _working = gameObject.FindChildWithName<Image>("Working");
        }

        public void UpdateArmour(ArmourController controller)
        {
            float leftOffset = 1 - controller.GetTotalProtection();
            _armourRect.anchorMin = new Vector2(leftOffset, 0f);
            _fillBar.fillAmount = controller.GetCurrentFill();
            _working.SetAlpha(controller.Recharging() ? 0f : 1f);
            if (!controller.DidJustTakeDamage()) return;
            _fillBar.color = Color.red;
            _fadeTween?.Complete();
            _fadeTween = _fillBar.DOColor(Color.white, 0.5f);
        }
    }
}