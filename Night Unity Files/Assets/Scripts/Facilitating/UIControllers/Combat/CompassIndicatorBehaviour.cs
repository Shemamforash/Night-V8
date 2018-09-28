using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class CompassIndicatorBehaviour : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private static readonly ObjectPool<CompassIndicatorBehaviour> _indicators = new ObjectPool<CompassIndicatorBehaviour>("Prefabs/Combat/Indicator");

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetAlpha(float alpha)
        {
            _spriteRenderer.color = new Color(1, 1, 1, alpha);
        }

        public void SetRotation(float rotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        
        private void Initialise(Transform parent)
        {
            transform.SetParent(parent);
            SetAlpha(0);
        }

        public void Disable()
        {
            _indicators.Return(this);
        }

        private void OnDestroy()
        {
            _indicators.Dispose(this);
        }

        public static CompassIndicatorBehaviour Create(Transform parent)
        {
            CompassIndicatorBehaviour indicator = _indicators.Create();
            indicator.Initialise(parent);
            return indicator;
        }
    }
}