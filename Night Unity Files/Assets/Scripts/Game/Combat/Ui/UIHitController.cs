using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Ui
{
    public class UIHitController : MonoBehaviour
    {
        private const float FadeTime = 0.5f;
        private float _currentShotTime;
        private Image _outerImage, _innerImage;

        public void Awake()
        {
            _innerImage = gameObject.FindChildWithName<Image>("Inner");
            _outerImage = gameObject.FindChildWithName<Image>("Outer");
        }

        public void Update()
        {
            float newHeight = 1f - PlayerCombat.Instance.GetRecoilModifier();
            _innerImage.fillAmount = newHeight;
            if (_currentShotTime <= 0) return;
            float rValue = 1 - _currentShotTime / FadeTime;
            _innerImage.color = new Color(1, rValue, rValue, 1);
            _outerImage.color = new Color(1, rValue, rValue, 1);
            _currentShotTime -= Time.deltaTime;
        }
    }
}