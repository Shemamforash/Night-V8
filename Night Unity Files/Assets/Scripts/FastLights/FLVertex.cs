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
        public bool OutOfRange;

        public FLVertex(Transform parentTransform, Vector3 localPosition)
        {
            _parentTransform = parentTransform;
            Position = _parentTransform.TransformPoint(localPosition);
        }

        public bool IsStart, IsEnd;

        public void SetDistanceAndAngle(Vector2 lightPosition, float range, Vector2 directionToLightPos)
        {
            SqrDistanceToOrigin = Vector2.SqrMagnitude(Position - lightPosition);
            OutOfRange = SqrDistanceToOrigin > range;
            Angle = 360 - AdvancedMaths.AngleFromUp(lightPosition, Position);
        }

        public void Draw()
        {
            Debug.DrawLine(Position, PreviousFlVertex.Position, Color.red, 5f);
            Debug.DrawLine(Position, NextFlVertex.Position, Color.red, 5f);
        }
    }
}