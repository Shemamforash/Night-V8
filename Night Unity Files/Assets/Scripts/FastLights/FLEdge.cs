using SamsHelper.Libraries;
using UnityEngine;

namespace FastLights
{
    public class FLEdge
    {
        public FLVertex From;
        public FLVertex To;
        
        public bool BelongsToEdge(FLVertex v)
        {
            return v == From || v == To;
        }

        public void SetVertices(FLVertex a, FLVertex b, Vector2 origin)
        {
            Vector2 midPoint = (a.Position + b.Position) / 2f;
            float dot = AdvancedMaths.Dot(origin, midPoint, a.Position);
            if (dot < 0)
            {
                From = a;
                To = b;
            }
            else
            {
                To = a;
                From = b;
            }
        }
    }
}