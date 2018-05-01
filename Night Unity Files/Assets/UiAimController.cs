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
        Vector3 forwardDir = transform.up;
        Vector3 start = transform.position + forwardDir * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(start, forwardDir, RayDistance, _layerMask);
        Vector3 end = start + forwardDir * RayDistance;
        if(hit.collider != null)
        {
            end = hit.point;
        }
        _lineRenderer.SetPositions(new[]{start, end});
    }
}
