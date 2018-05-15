using UnityEngine;

namespace FastLights
{
    public class FLVertex
    {
        public readonly Vector2 Position;
        public readonly float SqrDistanceToOrigin;
        public float SqrPerpendicularDistance;
        public readonly float Angle;
        public FLVertex PreviousFlVertex, NextFlVertex;

        public FLVertex(Vector2 position, float sqrDistanceToOrigin, float angle)
        {
            Position = position;
            SqrDistanceToOrigin = sqrDistanceToOrigin;
            Angle = angle;
        }
    }
}