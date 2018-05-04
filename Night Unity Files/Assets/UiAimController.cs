using Game.Combat.Enemies;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

public class UiAimController : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private const float RayDistance = 20f;
    private int _layerMask;

    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _layerMask = ~(1 << 9);
    }

    public void Update()
    {
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
    }
}