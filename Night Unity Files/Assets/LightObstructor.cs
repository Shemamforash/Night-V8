using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.Libraries;
using UnityEngine;

namespace DefaultNamespace
{
    public class LightObstructor : MonoBehaviour
    {
        [HideInInspector] public Mesh mesh;

        public void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            if (mesh == null) throw new UnnassignedMeshException();
            FastLight.RegisterObstacle(this);
        }

        private void OnDestroy()
        {
            FastLight.UnregisterObstacle(this);
        }

        public List<Vector2> GetVisibleVertices(Vector3 origin, float maxDistance)
        {
            List<Vector3> vertices = mesh.vertices.ToList();
            float biggestAngle = 0;
            Vector2 widestVertexA = Vector3.negativeInfinity;
            Vector2 widestVertexB = Vector3.negativeInfinity;
            float b = Vector2.Distance(origin, transform.position);
            float largestLeftAngle = 0;
            float largestRightAngle = 0;

            foreach (Vector3 v in vertices)
            {
                Vector3 worldVertex = transform.TransformPoint(v);
                float dot = AdvancedMaths.Dot(origin, transform.position, worldVertex);
                float c = Vector2.Distance(origin, worldVertex);
                if (c > maxDistance) continue;
                float a = Vector2.Distance(worldVertex, transform.position);
                float angle = AdvancedMaths.CosineRule(a, b, c);
                if (dot < 0)
                {
                    if (angle <= largestLeftAngle) continue;
                    largestLeftAngle = angle;
                    widestVertexA = worldVertex;
                }
                else
                {
                    if (angle <= largestRightAngle) continue;
                    largestRightAngle = angle;
                    widestVertexB = worldVertex;
                }
            }

//            for (int i = 0; i < vertices.Count; ++i)
//            {
//                Vector2 leftPoint = transform.TransformPoint(vertices[i]);
//                float b = Vector2.Distance(leftPoint, origin);
//                if (b > maxDistance) continue;
//                for (int j = i + 1; j < vertices.Count; ++j)
//                {
//                    Vector3 rightPoint = transform.TransformPoint(vertices[j]);
//                    float c = Vector2.Distance(rightPoint, origin);
//                    if (c > maxDistance) continue;
//                    float a = Vector2.Distance(leftPoint, rightPoint);
//                    float candidateAngle = AdvancedMaths.CosineRule(a, b, c);
////                    float candidateAngle = Vector2.Angle(origin - leftPoint, origin - rightPoint);
//                    if (candidateAngle <= biggestAngle) continue;
//                    biggestAngle = candidateAngle;
//                    widestVertexA = leftPoint;
//                    widestVertexB = rightPoint;
//                }
//            }
            return new List<Vector2> {widestVertexA, widestVertexB};
        }
    }

    public class UnnassignedMeshException : Exception
    {
        public string Message() => "No mesh assigned, a mesh is required for FastLight to work";
    }
}