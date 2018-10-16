using System.Collections;
using DG.Tweening;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper
{
    public class FadeAndDieTrailRenderer : MonoBehaviour
    {
        private static readonly ObjectPool<FadeAndDieTrailRenderer> _pathPool = new ObjectPool<FadeAndDieTrailRenderer>("Paths", "Prefabs/Borders/Path Trail Faded");
        private float _age;
        private float _opacityA, _opacityB;
        private TrailRenderer _trailRenderer;
        private bool Fading;
        public float LifeTime;
        private static readonly Color _red = new Color(1, 0, 0, 0.1f);
        private static readonly Color _pale = new Color(0.2f, 0.2f, 0.2f, 0.2f);

        public void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            _opacityA = _trailRenderer.startColor.a;
            _opacityB = _trailRenderer.endColor.a;
        }

        public static void CreatePale(Vector3[] rArr)
        {
            FadeAndDieTrailRenderer trail = _pathPool.Create();
            trail.Initialise(5f, _pale, rArr);
        }

        public static void CreateRed(Vector3[] rArr)
        {
            FadeAndDieTrailRenderer trail = _pathPool.Create();
            trail.Initialise(3f, _red, rArr);
        }

        private void Initialise(float duration, Color color, Vector3[] rArr)
        {
            transform.position = rArr[0];
            _trailRenderer.time = duration;
            _trailRenderer.startColor = color;
            _trailRenderer.endColor = color;
            _trailRenderer.Clear();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOPath(rArr, Random.Range(1f, 3f), PathType.CatmullRom, PathMode.TopDown2D));
            sequence.AppendCallback(() => StartFade(1f));
        }

        private void StartFade(float lifeTime)
        {
            if (Fading) return;
            Fading = true;
            LifeTime = lifeTime;
            _age = LifeTime;
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            while (_age > 0)
            {
                float normalisedLifeTime = _age / LifeTime;
                Color startColour = _trailRenderer.startColor;
                startColour.a = normalisedLifeTime * _opacityA;
                Color endColour = _trailRenderer.endColor;
                endColour.a = normalisedLifeTime * _opacityB;
                _trailRenderer.startColor = startColour;
                _trailRenderer.endColor = endColour;
                _age -= Time.deltaTime;
                yield return null;
            }

            _pathPool.Return(this);
        }

        private void OnDestroy()
        {
            _pathPool.Dispose(this);
        }
    }
}