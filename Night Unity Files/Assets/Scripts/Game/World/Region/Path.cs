using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World.Region
{
    public class Path : MonoBehaviour
    {
        private Vector2 _direction;
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        private Vector2 _currentPosition, _targetPosition;
        private const float MaxPathLength = 0.15f;
        private const float PathDrawTime = 0.1f;
        private float _currentTime;
        private bool _glowing;
        private float _glowAlpha = 1;

        public void DrawPath(MapNode from, MapNode to)
        {
            from.AddPathTo(to, this);
            to.AddPathTo(from, this);
            _targetPosition = to.transform.position;
            _currentPosition = from.transform.position;
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
                    bool passesTarget = CheckPassesThroughTarget(from, to);
                    if (passesTarget) to = _targetPosition;
                        segment.SetEnds(from, to);
                    if (passesTarget) break;
                    _currentPosition = to + _direction * MaxPathLength / 2f;
                    if (CheckPassesThroughTarget(to, _currentPosition)) break;
                    _currentTime = PathDrawTime;
                }

                yield return null;
            }
        }
        
        private bool CheckPassesThroughTarget(Vector2 from, Vector2 to)
        {
            float distanceAC = Vector2.Distance(from, _targetPosition);
            float distanceAB = Vector2.Distance(from, to);
            float distanceBC = Vector2.Distance(to, _targetPosition);
            float distanceDifference = distanceAC + distanceBC - distanceAB;
            return distanceDifference < 0.01f && distanceDifference > -0.01f;
        }
    }
}