using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Ui
{
    public class UIHitController : MonoBehaviour
    {
        private const float MaxHeight = 50f;
        private const float FadeTime = 0.5f;
        private float _currentShotTime;
        private RectTransform _innerRect;
        private Image _outerImage, _innerImage;

        public void Awake()
        {
            _innerRect = gameObject.FindChildWithName<RectTransform>("Inner");
            _innerImage = _innerRect.GetComponent<Image>();
            _outerImage = gameObject.FindChildWithName<Image>("Outer");
        }

        public void Update()
        {
            float newHeight = MaxHeight * (1 - PlayerCombat.Instance.GetAccuracyModifier());
            if (_currentShotTime > 0)
            {
                float rValue = 1 - _currentShotTime / FadeTime;
                _innerImage.color = new Color(1, rValue, rValue, 1);
                _outerImage.color = new Color(1, rValue, rValue, 1);
                _currentShotTime -= Time.deltaTime;
            }

            _innerRect.sizeDelta = new Vector2(newHeight, newHeight);
        }

        public void RegisterShot()
        {
            _currentShotTime = FadeTime;
        }
    }
}