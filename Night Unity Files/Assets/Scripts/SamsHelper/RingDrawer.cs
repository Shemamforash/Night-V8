﻿namespace SamsHelper
{
    using UnityEngine;

    [RequireComponent(typeof(LineRenderer))]
    public class RingDrawer : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private const int Segments = 128;
        private const int Overlap = 1;
        private float _angleDelta;

        public void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = Segments + Overlap;
            _lineRenderer.useWorldSpace = false;
            _angleDelta = 2 * Mathf.PI / Segments;
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
            Color invisible = new Color(1f, 1f, 1f, 0f);
            _lineRenderer.startColor = invisible;
            _lineRenderer.endColor = invisible;
        }

        public void SetFaded()
        {
            Color invisible = new Color(1f, 1f, 1f, 0.4f);
            _lineRenderer.startColor = invisible;
            _lineRenderer.endColor = invisible;
        }

        public void SetHighlighted()
        {
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
        }

        public void DrawCircle(float radius)
        {
            float currentAngle = 0f;
            for (int i = 0; i < Segments + Overlap; i++)
            {
                float x = radius * Mathf.Cos(currentAngle);
                float z = radius * Mathf.Sin(currentAngle);
                Vector3 pos = new Vector3(x, 0, z);
                _lineRenderer.SetPosition(i, pos);
                currentAngle += _angleDelta;
            }
        }
    }
}