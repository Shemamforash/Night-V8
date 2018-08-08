using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentSegmentBehaviour : MonoBehaviour
{
    protected SerpentSegmentBehaviour NextSegment;
    private Vector3 _lastPosition;
    private float _lastRotation;
    private readonly Queue<Vector3> _parentPositions = new Queue<Vector3>();
    private readonly Queue<float> _parentRotations = new Queue<float>();

    public void SetPosition(Vector2 position, float rotation)
    {
        _parentPositions.Enqueue(position);
        _parentRotations.Enqueue(rotation);

        if (_parentPositions.Count < 10) return;

        float newRotation = _parentRotations.Dequeue();
        Vector3 newPosition = _parentPositions.Dequeue();
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
        transform.position = newPosition;

        SetNextSegmentPosition(newPosition, newRotation);
    }

    protected void SetNextSegment(Transform root, int i)
    {
        transform.position = root.transform.position;
        Transform wingObject = root.Find("Wing Segment (" + i + ")");
        if (wingObject == null)
        {
            switch (gameObject.name)
            {
                case "Tail Upper Spine":
                    wingObject = root.FindChildWithName("Tail Mid Spine");
                    break;
                case "Tail Mid Spine":
                    wingObject = root.FindChildWithName("Tail End");
                    break;
                case "Tail End":
                    return;
                default:
                    wingObject = root.FindChildWithName("Tail Upper Spine");
                    break;
            }
        }
        NextSegment = wingObject.GetComponent<SerpentSegmentBehaviour>();
        if (NextSegment == null) return;
        NextSegment.SetNextSegment(root, i + 1);
    }
    
    private void SetNextSegmentPosition(Vector3 newPosition, float newRotation)
    {
        if (NextSegment == null) return;
        NextSegment.SetPosition(_lastPosition, _lastRotation);
        _lastRotation = newRotation;
        _lastPosition = newPosition;
    }
}