using System.Collections;
using Game.World.Region;
using UnityEngine;

public class UiPathDrawController : MonoBehaviour
{
    private const float MaxPathLength = 0.15f;
    private const float PathDrawTime = 0.1f;
    private float _currentTime;
    private Vector2 _direction;
    private Vector2 _currentPosition, _targetPosition;

    public static void CreatePathBetweenNodes(MapNode from, MapNode to)
    {
        if (from.Links.Contains(to)) return;
        from.Links.Add(to);
        to.Links.Add(from);
        GameObject pathObject = new GameObject();
        pathObject.transform.SetParent(GameObject.Find("Path").transform);
        pathObject.transform.position = Vector2.zero;
        UiPathDrawController pathController = pathObject.AddComponent<UiPathDrawController>();
        pathController.DrawPath(from, to);
    }

    private void DrawPath(MapNode from, MapNode to)
    {
        _targetPosition = to.transform.position;
        _currentPosition = from.transform.position;
        _direction = _targetPosition - _currentPosition;
        _currentTime = 0f;
        _direction.Normalize();
        StartCoroutine(UpdatePath());
    }

    private class PathFade : MonoBehaviour
    {
        private const float FullOpacity = 0.3f;
        private float _age;
        private const float FadeInTime = 2f;
        private LineRenderer _lineRenderer;

        public void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            SetColor(0);
        }

        private void SetColor(float alpha)
        {
            Color c = new Color(1, 1, 1, alpha);
            _lineRenderer.startColor = c;
            _lineRenderer.endColor = c;
        }

        public void StartFade(Vector3 from, Vector3 to)
        {
            _lineRenderer.SetPositions(new[] {from, to});
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            while (true)
            {
                float normalisedLifeTime = _age / FadeInTime;
                _age += Time.deltaTime;
                if (_age > FadeInTime) _age = FadeInTime;
                SetColor(normalisedLifeTime * FullOpacity);
                if (_age >= FadeInTime)
                {
                    break;
                }

                yield return null;
            }
        }
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
                PathFade fade = pathSegment.AddComponent<PathFade>();

                Vector2 from = _currentPosition;
                Vector2 to = _currentPosition + _direction * MaxPathLength;
                bool passesTarget = CheckPassesThroughTarget(from, to);
                if (passesTarget) to = _targetPosition;
                fade.StartFade(from, to);
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