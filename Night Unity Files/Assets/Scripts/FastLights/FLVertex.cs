using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace FastLights
{
    public class FLVertex
    {
        public readonly Vector2 Position;
        public float SqrDistanceToOrigin;
        public FLVertex PreviousFlVertex, NextFlVertex;
        public Vector2 InRangePosition;
        public float InRangeAngle;
        public bool OutOfRange;
        private List<FLVertex> _segment;

        public FLVertex(Transform parentTransform, Vector3 localPosition)
        {
            Position = parentTransform.TransformPoint(localPosition);
        }

        public bool IsStart, IsEnd;

        public void SetDistanceAndAngle(Vector2 lightPosition, float range)
        {
            IsStart = false;
            IsEnd = false;
            SqrDistanceToOrigin = Vector2.SqrMagnitude(Position - lightPosition);
            OutOfRange = SqrDistanceToOrigin > range;
            InRangePosition = Position;
            InRangeAngle = 360 - AdvancedMaths.AngleFromUp(lightPosition, Position);
        }

        public void SetInRangePosition(Vector2 inRangePosition, Vector2 lightPosition)
        {
            InRangePosition = inRangePosition;
            SqrDistanceToOrigin = Vector2.SqrMagnitude(Position - lightPosition);
            InRangeAngle = 360 - AdvancedMaths.AngleFromUp(lightPosition, InRangePosition);
        }
        
        public void Draw()
        {
            Debug.DrawLine(Position, PreviousFlVertex.Position, Color.red, 5f);
            Debug.DrawLine(Position, NextFlVertex.Position, Color.red, 5f);
        }
    }
}