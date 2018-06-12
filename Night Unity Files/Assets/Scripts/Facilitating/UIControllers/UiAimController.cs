using Game.Combat.Enemies;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

public class UiAimController : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private const float RayDistance = 20f;
    private int _layerMask;
    private static float _alpha;

    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _layerMask = ~((1 << 9) | (1 << 12));
    }

    public void Update()
    {
        if (_alpha == 0) return;
        if (CombatManager.AllEnemiesDead())
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;
        Vector3 forwardDir = transform.up;
        Vector3 start = transform.position + forwardDir * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(start, forwardDir, RayDistance, _layerMask);
        Vector3 end = start + forwardDir * RayDistance;
        if (hit.collider != null && hit.collider.CompareTag("UiCollider"))
        {
            end = hit.point;
            EnemyBehaviour enemyBehaviour = hit.collider.transform.parent.GetComponent<EnemyBehaviour>();
            CombatManager.Player().SetTarget(enemyBehaviour);
        }
        else
        {
            CombatManager.Player().SetTarget(null);
        }

        _lineRenderer.SetPositions(new[] {start, end});
        _lineRenderer.startColor = new Color(1,1,1, _alpha * 0.2f);
        _lineRenderer.endColor = new Color(1, 1, 1, _alpha * 0.1f);
    }

    public static void SetAlpha(float alpha)
    {
        _alpha = alpha;
    }
}