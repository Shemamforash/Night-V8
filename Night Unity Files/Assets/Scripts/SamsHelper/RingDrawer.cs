using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace SamsHelper
{
    [RequireComponent(typeof(LineRenderer))]
    public class RingDrawer : MonoBehaviour
    {
        private const int Segments = 128;
        private const int Overlap = 1;
        private float _angleDelta;
        private float _lastRadius = -1;
        private LineRenderer _lineRenderer;
        [Range(0, 15)] public float radius;
        public Material _material;

        public void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = Segments + Overlap;
            _lineRenderer.useWorldSpace = false;
            _angleDelta = 2 * Mathf.PI / Segments;
            _lineRenderer.material = _material;
            DrawCircle(radius);
        }

        public void SetLineWidth(float lineWidth)
        {
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
        }

        public void SetColor(Color c)
        {
            _lineRenderer.startColor = c;
            _lineRenderer.endColor = c;
        }

        public void Hide()
        {
            Color invisible = UiAppearanceController.InvisibleColour;
            _lineRenderer.startColor = invisible;
            _lineRenderer.endColor = invisible;
        }

//        public void Update()
//        {
//            if (_lastRadius != -1 && _lastRadius != radius)
//            {
//                DrawCircle(radius);
//            }
//
//            _lastRadius = radius;
//        }

        public void DrawCircle(float radius)
        {
            float currentAngle = 0f;
            for (int i = 0; i < Segments + Overlap; i++)
            {
                float x = radius * Mathf.Cos(currentAngle);
                float y = radius * Mathf.Sin(currentAngle);
                Vector3 pos = new Vector3(x, y, 0);
                _lineRenderer.SetPosition(i, pos);
                currentAngle += _angleDelta;
            }
        }
    }
}