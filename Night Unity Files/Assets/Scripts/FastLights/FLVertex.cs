using UnityEngine;

namespace FastLights
{
    public class FLVertex
    {
        public readonly Vector2 Position;
        public readonly float DistanceToOrigin;
        public float PerpendicularDistance;
        public readonly float Angle;
        public FLVertex PreviousFlVertex, NextFlVertex;

        public FLVertex(Vector2 position, float distanceToOrigin, float angle)
        {
            Position = position;
            DistanceToOrigin = distanceToOrigin;
            Angle = angle;
        }
    }
}