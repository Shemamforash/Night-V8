using DG.Tweening;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class CompassItem : MonoBehaviour
    {
        private static GameObject _glowPrefab;
        private SpriteRenderer _glow;

        private void Awake()
        {
            if(_glowPrefab == null) _glowPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Glow");
            GameObject glowObject = Instantiate(_glowPrefab);
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector2.zero;
            _glow = glowObject.GetComponent<SpriteRenderer>();
            _glow.color = UiAppearanceController.InvisibleColour;
        }

        public void Start()
        {
            UiCompassController.RegisterCompassItem(this);
        }

        public void OnDestroy()
        {
            UiCompassController.UnregisterCompassItem(this);
        }

        public void Die()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_glow.DOFade(0f, 1f));
            sequence.AppendCallback(() => Destroy(_glow.gameObject));
            Destroy(this);
        }

        public void Pulse()
        {
            _glow.color = new Color(1, 1, 1, 0.3f);
            _glow.DOFade(0f, 2f);
        }
    }
}