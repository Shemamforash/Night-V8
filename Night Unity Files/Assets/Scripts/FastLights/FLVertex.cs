using UnityEngine;

namespace FastLights
{
    public class FLVertex
    {
        public readonly Vector2 Position;
        public readonly float Distance;
        public FLEdge FlEdge;
        public readonly float Angle;
        public FLVertex PreviousFlVertex, NextFlVertex;

        public FLVertex(Vector2 position, float distance, float angle)
        {
            Position = position;
            Distance = distance;
            Angle = angle;
        }
    }
}