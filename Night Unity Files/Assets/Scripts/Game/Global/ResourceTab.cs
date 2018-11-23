using DG.Tweening;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Global
{
    public class ResourceTab : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Image _glow;
        private Tweener _textTween, _glowTween;
        private int _lastValue;

        public void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _glow = gameObject.FindChildWithName<Image>("Glow");
        }

        public void UpdateTab(string resourceName, int quantity)
        {
            if (quantity == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (quantity == _lastValue) return;
            _text.text = quantity + " " + resourceName;
            if (quantity < _lastValue)
            {
                _textTween?.Complete();
                _text.color = Color.red;
                _textTween = _text.DOColor(Color.white, 1f);
            }
            else
            {
                _glowTween?.Complete();
                _glow.SetAlpha(0.5f);
                _glowTween = _glow.DOFade(0f, 1f);
            }

            _lastValue = quantity;
        }
    }
}