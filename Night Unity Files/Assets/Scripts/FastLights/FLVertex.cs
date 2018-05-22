using SamsHelper.Libraries;
using UnityEngine;

namespace FastLights
{
    public class FLVertex
    {
        private Transform _parentTransform;
        public readonly Vector2 Position;
        public float SqrDistanceToOrigin;
        public float Angle;
        public FLVertex PreviousFlVertex, NextFlVertex;
        public Vector2 InRangePosition;
        public float InRangeAngle;
        public bool OutOfRange;
        public FLEdge EdgeA, EdgeB;

        public FLVertex(Transform parentTransform, Vector3 localPosition)
        {
            _parentTransform = parentTransform;
            Position = _parentTransform.TransformPoint(localPosition);
        }

        public bool IsStart, IsEnd;

        public bool SharesTransform(FLVertex v)
        {
            return v._parentTransform == _parentTransform;
        }
        
        public void SetDistanceAndAngle(Vector2 lightPosition, float range)
        {
            IsStart = false;
            IsEnd = false;
            SqrDistanceToOrigin = Vector2.SqrMagnitude(Position - lightPosition);
            OutOfRange = SqrDistanceToOrigin > range;
            Angle = 360 - AdvancedMaths.AngleFromUp(lightPosition, Position);
            InRangePosition = Position;
            InRangeAngle = Angle;
        }

        public void SetInRangePosition(Vector2 inRangePosition, Vector2 lightPosition)
        {
            InRangePosition = inRangePosition;
            InRangeAngle = 360 - AdvancedMaths.AngleFromUp(lightPosition, InRangePosition);
        }
        
        public void Draw()
        {
            Debug.DrawLine(Position, PreviousFlVertex.Position, Color.red, 5f);
            Debug.DrawLine(Position, NextFlVertex.Position, Color.red, 5f);
        }
    }
}