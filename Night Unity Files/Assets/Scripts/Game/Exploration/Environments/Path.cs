using System.Collections;
using System.Collections.Generic;
using Game.Exploration.Regions;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class Path : MonoBehaviour
    {
        private const float MaxPathLength = 0.15f;
        private const float PathDrawTime = 0.1f;
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        private Vector2 _currentPosition, _targetPosition;
        private float _currentTime;
        private Vector2 _direction;
        private float _glowAlpha = 1;
        private bool _glowing;

        public void DrawPath(Region from, Region to)
        {
            from.AddPathTo(to, this);
            to.AddPathTo(from, this);
            _targetPosition = to.Position;
            _currentPosition = from.Position;
            _direction = _targetPosition - _currentPosition;
            _currentTime = 0f;
            _direction.Normalize();
            StartCoroutine(UpdatePath());
        }

        public void GlowSegments(float alpha)
        {
            _glowAlpha = alpha;
            _glowing = true;
        }

        public void StopGlowing()
        {
            _glowAlpha = 1;
            _glowing = false;
        }

        public void Update()
        {
            _segments.ForEach(s => s.UpdateColor(_glowAlpha, _glowing));
        }

        private IEnumerator UpdatePath()
        {
            while (true)
            {
                _currentTime -= Time.deltaTime;
                if (_currentTime <= 0f)
                {
                    GameObject pathSegment = Instantiate(Resources.Load("Prefabs/Map/Path Segment") as GameObject);
                    pathSegment.transform.SetParent(transform);
                    pathSegment.transform.rotation = Quaternion.LookRotation(_direction);
                    PathSegment segment = pathSegment.AddComponent<PathSegment>();
                    _segments.Add(segment);

                    Vector2 from = _currentPosition;
                    Vector2 to = _currentPosition + _direction * MaxPathLength;
                    bool passesTarget = AdvancedMaths.DoesLinePassThroughPoint(from, to, _targetPosition);
                    if (passesTarget) to = _targetPosition;
                    segment.SetEnds(from, to);
                    if (passesTarget) break;
                    _currentPosition = to + _direction * MaxPathLength / 2f;
                    if (AdvancedMaths.DoesLinePassThroughPoint(to, _currentPosition, _targetPosition)) break;
                    _currentTime = PathDrawTime;
                }

                yield return null;
            }
        }
    }
}