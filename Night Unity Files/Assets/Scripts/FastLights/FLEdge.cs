using UnityEngine;

namespace FastLights
{
    public class FLEdge
    {
        public readonly FLVertex From, To;
        private bool Visible;

        public FLEdge(FLVertex from, FLVertex to)
        {
            From = from;
            To = to;
        }

        public bool CalculateVisibility(Vector2 origin)
        {
            Vector2 normal = From.Position - To.Position;
            normal = Quaternion.AngleAxis(90, Vector3.forward) * normal;
            Vector2 mid = (From.Position + To.Position) / 2f;
            float dot = Vector3.Dot(normal, origin - mid);
            Visible = dot > 0;
            return Visible;
        }

        public bool BelongsToEdge(FLVertex v)
        {
            return v == From || v == To;
        }

        public void Draw()
        {
            Vector3 mid = (From.Position + To.Position) / 2f;
            Debug.DrawLine(From.Position, mid, Color.red, 10f);
            Debug.DrawLine(mid, To.Position, Color.green, 10f);
        }
    }
}