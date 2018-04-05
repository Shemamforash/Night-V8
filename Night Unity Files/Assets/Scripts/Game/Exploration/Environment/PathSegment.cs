using UnityEngine;

namespace Game.World.Region
{
    public class PathSegment : MonoBehaviour
    {
        private const float FullOpacity = 0.3f;
        private float _age;
        private const float FadeInTime = 2f;
        private LineRenderer _lineRenderer;
        private bool _fading = true;
        private int bgComponents = 1;
        private float lastAlpha = -1;

        public void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            SetColor(0);
        }

        public void SetColor(float alpha, bool shouldGlow = false)
        {
            if(!shouldGlow) alpha *= FullOpacity;
            bgComponents = shouldGlow ? 0 : 1;
            Color c = new Color(1, bgComponents, bgComponents, alpha);
            _lineRenderer.startColor = c;
            _lineRenderer.endColor = c;
        }

        public void SetEnds(Vector3 from, Vector3 to)
        {
            _lineRenderer.SetPositions(new[] {from, to});
        }

        public void UpdateColor(float alpha = 1, bool glow = false)
        {
            if (_fading)
            {
                if (_age > FadeInTime) _age = FadeInTime;
                float normalisedLifeTime = _age / FadeInTime;
                alpha *= normalisedLifeTime;
                if (_age >= FadeInTime)
                {
                    _fading = false;
                }

                _age += Time.deltaTime;
            }

            if (lastAlpha != alpha)
            {
                SetColor(alpha, glow);
            }

            lastAlpha = alpha;
        }
    }
}