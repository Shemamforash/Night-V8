using System;
using System.Collections.Generic;
using System.Net;
using FastLights;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Fastlights
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FastLight : MonoBehaviour
    {
        private static readonly List<FastLight> _lights = new List<FastLight>();
        private static readonly List<LightObstructor> _allObstructors = new List<LightObstructor>();

        private Vector2 _position = Vector2.negativeInfinity;
        private bool _needsUpdate;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Color _lastColour;
        private float _lastRadius;

        public Color Colour = Color.white;
        public float Radius;
        public Material LightMaterial;
        public GameObject Target;

        public static void UpdateLights()
        {
            _lights.ForEach(l => l._needsUpdate = true);
        }

        public void Awake()
        {
            _lights.Add(this);
            if (LightMaterial == null) LightMaterial = new Material(Shader.Find("Unlit/Texture"));
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer.material = LightMaterial;
            Vector3 position = transform.position;
            transform.position = position;
        }

        private void OnDestroy()
        {
            _lights.Remove(this);
        }

        public static void RegisterObstacle(LightObstructor lightObstructor)
        {
            _allObstructors.Add(lightObstructor);
            UpdateLights();
        }

        public static void UnregisterObstacle(LightObstructor lightObstructor)
        {
            _allObstructors.Remove(lightObstructor);
        }

        private Tuple<bool, Vector2, float> GetIntersectionWithEdge(Vector2 end, List<FLEdge> edges, FLVertex flVertex)
        {
            Vector2 nearestIntersection = Vector2.negativeInfinity;
            float nearestSqrDistance = float.MaxValue;
            bool intersectionExists = false;
            float rayAngle = 360 - AdvancedMaths.AngleFromUp(_position, end);
            edges.ForEach(e =>
            {
                if (e.BelongsToEdge(flVertex)) return;
                float fromAngle = e.From.InRangeAngle;
                float toAngle = e.To.InRangeAngle;
                float tempRayAngle = rayAngle;
                if (fromAngle > toAngle)
                {
                    if (tempRayAngle > fromAngle && tempRayAngle > toAngle)
                    {
                        tempRayAngle -= 360;
                    }

                    fromAngle -= 360f;
                }

                if (fromAngle > tempRayAngle && toAngle > tempRayAngle) return;
                if (fromAngle < tempRayAngle && toAngle < tempRayAngle) return;
                Tuple<bool, Vector2> intersection = AdvancedMaths.LineIntersection(e.From.InRangePosition, e.To.InRangePosition, _position, end);
                if (!intersection.Item1) return;
                float sqrDistance = Vector2.SqrMagnitude(_position - intersection.Item2);
                if (sqrDistance >= nearestSqrDistance) return;
                nearestSqrDistance = sqrDistance;
                intersectionExists = true;
                nearestIntersection = intersection.Item2;
            });
            return new Tuple<bool, Vector2, float>(intersectionExists, nearestIntersection, nearestSqrDistance);
        }

        private bool CheckIsNan(Vector2 vect)
        {
            return float.IsNaN(vect.x) || float.IsNaN(vect.y);
        }

        private List<List<FLEdge>> _visibleEdges;
        private Tuple<bool, Vector2, float> _intersection;

        private void CheckIsEndOfSegment(int current, int next, List<FLVertex> currentSegment, List<FLVertex> nextSegment)
        {
            if (current != currentSegment.Count - 1) return;
            Vector2 dirToVertex = (currentSegment[current].InRangePosition - _position).normalized;
            Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
            if (next == 0)
            {
                if (nextSegment[next].SqrDistanceToOrigin < currentSegment[current].SqrDistanceToOrigin && nextSegment[next].SharesTransform(currentSegment[current])) return;
                meshVertices.Add(currentSegment[current].InRangePosition);
                meshVertices.Add(lineSegmentEnd);
                Vector2 dirToNextVertex = (nextSegment[next].InRangePosition - _position).normalized;
                Vector2 nextLineSegmentEnd = _position + dirToNextVertex * Radius;
                meshVertices.Add(nextLineSegmentEnd);
                meshVertices.Add(nextSegment[next].InRangePosition);
            }
            else
            {
//                Tuple<bool, Vector2> intersect = AdvancedMaths.LineIntersection(_position, lineSegmentEnd, nextSegment[next].InRangePosition, nextSegment[next - 1].InRangePosition);
//                meshVertices.Add(intersect.Item2);
            }
        }

        private List<Vector2> meshVertices = new List<Vector2>();

        private Tuple<bool, Vector2> Intersection(Vector2 origin, FLVertex target, List<FLEdge> edges)
        {
            Tuple<bool, Vector2> intersect = Tuple.Create(false, Vector2.zero);
            foreach (FLEdge e in edges)
            {
                if (e.BelongsToEdge(target)) continue;
                intersect = AdvancedMaths.LineIntersection(origin, target.InRangePosition, e.From.InRangePosition, e.To.InRangePosition);
                if (intersect.Item1) break;
            }

            return intersect;
        }
        
        private void DrawLight()
        {
            meshVertices.Clear();
            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();

            float sqrRadius = Radius * Radius;
            _allObstructors.ForEach(o =>
            {
                float sqrDistanceToMesh = Vector2.SqrMagnitude((Vector2) o.transform.position - _position);
                if (sqrDistanceToMesh - o.MaxRadius() > sqrRadius) return;
                _visibleEdges = o.GetVisibleVertices(_position, sqrRadius, Radius);
                edgeSegments.AddRange(_visibleEdges);
            });

//            for (int i = 0; i < edgeSegments.Count; ++i)
//            {
//                float pos = (float) i / edgeSegments.Count;
//                Color c = Color.Lerp(Color.red, Color.green, pos);
//                for (int j = 0; j < edgeSegments[i].Count; ++j)
//                {
//                    Debug.DrawLine(edgeSegments[i][j].From.InRangePosition, edgeSegments[i][j].To.InRangePosition, c, 5f);
//                }
//            }

            if (edgeSegments.Count == 0)
            {
                InsertLineSegments(meshVertices, 0, 360);
                return;
            }

            List<FLEdge> es = new List<FLEdge>();
            
            List<List<FLVertex>> vertSegments = new List<List<FLVertex>>();
            List<FLVertex> verts = new List<FLVertex>();
            edgeSegments.ForEach(s =>
            {
                List<FLVertex> segment = new List<FLVertex>();
                vertSegments.Add(segment);
                for (int i = 0; i < s.Count; ++i)
                {
                    es.Add(s[i]);
                    segment.Add(s[i].From);
                    verts.Add(s[i].From);
                    s[i].From.SetSegment(segment, segment.Count - 1);
                    if (i != s.Count - 1) continue;
                    segment.Add(s[i].To);
                    verts.Add(s[i].To);
                    s[i].To.SetSegment(segment, segment.Count - 1);
                }
            });
            verts.Sort((a, b) => a.Angle.CompareTo(b.Angle));
            
            verts.ForEach(vert =>
            {
                Tuple<bool, Vector2> inter= Intersection(_position, vert, es);
//                if(inter.Item1) Debug.DrawLine(_position, inter.Item2, Color.yellow, 5f);
//                else
//                {
//                    Vector2 dirToVertex = (vert.InRangePosition - _position).normalized;
//                    Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
//                    Debug.DrawLine(_position, vert.InRangePosition, Color.yellow, 5f);
//                }
            });
            
            FLVertex nearest = verts[0];
            float nearestDistance = nearest.SqrDistanceToOrigin;
            for (int i = 1; i < verts.Count; ++i)
            {
                verts[i].vertIndex = i;
                if (verts[i].SqrDistanceToOrigin >= nearestDistance) continue;
                nearest = verts[i];
                nearestDistance = verts[i].SqrDistanceToOrigin;
            }

            for (int i = 0; i < vertSegments.Count; ++i)
            {
                for (int j = 0; j < vertSegments[i].Count - 1; ++j)
                {
                    float pos = (float) j / vertSegments[i].Count;
                    Color c = Color.Lerp(Color.red, Color.green, pos);
                    Debug.DrawLine(vertSegments[i][j].InRangePosition, vertSegments[i][j + 1].InRangePosition, c, 5f);
                }
            }

            vertSegments.Sort((a, b) => a[0].SqrDistanceToOrigin.CompareTo(b[0].SqrDistanceToOrigin));

            List<FLVertex> currentSegment = nearest.Segment();
            int vIndex = Helper.NextIndex(nearest.vertIndex, verts);
            FLVertex nextVert = verts[vIndex];
            
            Debug.Log("running");
            
            for (int j = nearest.segmentIndex + 1; j < currentSegment.Count; ++j)
            {
                FLVertex edgeStart = currentSegment[j - 1];
                FLVertex edgeEnd = currentSegment[j];
                if (edgeEnd == nextVert) //if the next vertex is the end of the current edge, we can just add the edge
                {
                    meshVertices.Add(edgeStart.InRangePosition);
                    meshVertices.Add(edgeEnd.InRangePosition);
                    vIndex = Helper.NextIndex(vIndex, verts);
                    nextVert = verts[vIndex];
                    if (j != currentSegment.Count) continue;
                    currentSegment = nextVert.Segment();
                    j = nextVert.segmentIndex;
                }
                else
                {
                    while (edgeEnd != nextVert) //otherwise we inspect each vertex until we reach the end of the current edge, or we find a vertex obstructing it
                    {
                        //get the intersection of the current edge with the next vertex
                        Tuple<bool, Vector2> intersection = AdvancedMaths.LineIntersection(edgeStart.InRangePosition, edgeEnd.InRangePosition, nextVert.InRangePosition, _position);
                        //if an intersection occurs, the next vertex must lie beyond the current edge, and therefore be invisible, we can move on
                        if (intersection.Item1) // point is further
                        {
                            vIndex = Helper.NextIndex(vIndex, verts);
                            nextVert = verts[vIndex];
                        }
                        else
                        {
                            Vector2 dirToVertex = (nextVert.InRangePosition - _position).normalized;
                            Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
                            intersection = AdvancedMaths.LineIntersection(edgeStart.InRangePosition, edgeEnd.InRangePosition, lineSegmentEnd, _position);
                            if (!intersection.Item1)
                            {
                                intersection = Tuple.Create(true, nextVert.InRangePosition);
//                                    Debug.DrawLine(edgeStart.InRangePosition, edgeEnd.InRangePosition, Color.red, 5f);
//                                    Debug.DrawLine(nextVert.InRangePosition, _position, Color.red, 5f);
                            }

                            meshVertices.Add(intersection.Item2);
                            meshVertices.Add(nextVert.InRangePosition);
                            currentSegment = nextVert.Segment();
                            j = nextVert.segmentIndex;
                            vIndex = Helper.NextIndex(nextVert.vertIndex, verts);
                            nextVert = verts[vIndex];
                            break;
                        }
                    }

                    meshVertices.Add(edgeStart.InRangePosition);
                    meshVertices.Add(edgeEnd.InRangePosition);
                }
            }

            for (int i = 0; i < meshVertices.Count; ++i)
            {
                Vector3 next = Helper.NextElement(i, meshVertices);
//                Debug.DrawLine(meshVertices[i], next, Color.blue, 5f);
            }

            Mesh mesh = _meshFilter.mesh;
            mesh.Clear();
            meshVertices.Insert(0, _position);
            Vector3[] v = new Vector3[meshVertices.Count];
            for (int i = 0; i < meshVertices.Count; ++i)
            {
                Vector3 vert = transform.InverseTransformPoint(meshVertices[i]);
                vert.z = 2;
                v[i] = vert;
            }

            mesh.vertices = v;
            mesh.triangles = Triangulate(v);
            Vector3[] normals = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector2.up;
            mesh.normals = normals;
            mesh.uv = CalculateUvs(mesh.vertices);
        }


//        private void DrawLight()
//        {
//            meshVertices.Clear();
//            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();
//
//            float sqrRadius = Radius * Radius;
//            _allObstructors.ForEach(o =>
//            {
//                float sqrDistanceToMesh = Vector2.SqrMagnitude((Vector2) o.transform.position - _position);
//                if (sqrDistanceToMesh - o.MaxRadius() > sqrRadius) return;
//                _visibleEdges = o.GetVisibleVertices(_position, sqrRadius);
//                edgeSegments.AddRange(_visibleEdges);
//            });
//
//            List<int> positionsToRemoveAt = new List<int>();
//            for (int i = 0; i < edgeSegments.Count; ++i)
//            {
//                float angleFromA = edgeSegments[i][0].From.Angle;
//                int lastSegmentIndex = edgeSegments[i].Count - 1;
//                float angleToA = edgeSegments[i][lastSegmentIndex].To.Angle;
//                if (angleFromA > angleToA) angleFromA = angleFromA - 360f;
//
//                for (int j = i + 1; j < edgeSegments.Count; ++j)
//                {
//                    if (i == j || positionsToRemoveAt.Contains(j)) continue;
//                    float angleFromB = edgeSegments[j][0].From.Angle;
//                    lastSegmentIndex = edgeSegments[j].Count - 1;
//                    float angleToB = edgeSegments[j][lastSegmentIndex].To.Angle;
//                    if (angleFromB > angleToB) angleFromB = angleFromB - 360f;
//
//                    if (angleFromB < angleFromA || angleToB > angleToA) continue;
////                    positionsToRemoveAt.Add(j);
//                }
//            }
//
//            //try walk along edge algorithm
//            edgeSegments.Sort((a, b) => a[0].From.Angle.CompareTo(b[0].From.Angle));
//
//            for (int i = 0; i < edgeSegments.Count; ++i)
//            {
//                float pos = (float) i / edgeSegments.Count;
//                Color c = Color.Lerp(Color.red, Color.green, pos);
//                for (int j = 0; j < edgeSegments[i].Count; ++j)
//                {
//                    Debug.DrawLine(edgeSegments[i][j].From.InRangePosition, edgeSegments[i][j].To.InRangePosition, c, 5f);
//                }
//            }
//
//            List<List<FLVertex>> vertSegments = new List<List<FLVertex>>();
//            edgeSegments.ForEach(s =>
//            {
//                List<FLVertex> segment = new List<FLVertex>();
//                vertSegments.Add(segment);
//                for (int i = 0; i < s.Count; ++i)
//                {
//                    segment.Add(s[i].From);
//                    if (i == s.Count - 1) segment.Add(s[i].To);
//                }
//            });
//
//            for (int i = 0; i < vertSegments.Count; ++i)
//            {
//                List<FLVertex> currentSegment = vertSegments[i];
//                List<FLVertex> nextSegment = Helper.NextElement(i, vertSegments);
//                currentSegment.ForEach(currentVert =>
//                {
//                    for (int j = nextSegment.Count - 1; j >= 0; --j)
//                    {
//                        FLVertex vert = nextSegment[j];
//                        if (currentVert.Angle > vert.Angle)
//                        {
//                            if (vert.SqrDistanceToOrigin < currentVert.SqrDistanceToOrigin)
//                            {
//                                //move to this segment
//                            }
//                            else
//                            {
//                                nextSegment.Remove(vert);
//                            }
//                        }
//                        else
//                        {
//                            break;
//                        }
//                    }
//
//                    float nextVertexAngle = nextSegment[0].From.Angle;
//                    while (nextVertexAngle < v.From.Angle)
//                    {
//                    }
//
//                    if (v.From.Angle > nextSegment[0].From.Angle)
//                    {
//                        if (v.From.SqrDistanceToOrigin < nextSegment[0].From.SqrDistanceToOrigin)
//                        {
//                        }
//                    }
//                });
//            }
//
//            positionsToRemoveAt.Sort();
//            for (int i = positionsToRemoveAt.Count - 1; i >= 0; --i)
//            {
//                edgeSegments.RemoveAt(positionsToRemoveAt[i]);
//            }
//
//            edgeSegments.ForEach(s =>
//            {
//                Color a = Helper.RandomColour();
//                Color b = Helper.RandomColour();
////                Debug.Log(s[0].From.IsStart + " " + s[s.Count-1].To.IsEnd);
////                Debug.DrawLine(s[0].From.Position, _position, Color.yellow, 10f);
////                Debug.DrawLine(s[s.Count - 1].To.Position, _position, Color.magenta, 10f);
////                s.ForEach(e => e.Draw(a, b));
//            });
//
//            if (edgeSegments.Count == 0)
//            {
//                InsertLineSegments(meshVertices, 0, 360);
//            }
//
//            //works to here
//
//            List<FLVertex> vertices = new List<FLVertex>();
//            List<FLEdge> edges = new List<FLEdge>();
//
//            edgeSegments.ForEach(s => s.ForEach(e =>
//            {
//                if (e.From.OutOfRange)
//                {
//                    Color c = Helper.RandomColour();
////                    Debug.DrawLine(e.From.InRangePosition, _position, c, 5f);
//                    Vector2 newVertexPos = AdvancedMaths.FindLineSegmentCircleIntersections(e.From.Position, e.To.Position, _position, Radius)[0];
//                    e.From.SetInRangePosition(newVertexPos, _position);
//                    c.a = 0.5f;
////                    Debug.DrawLine(e.From.InRangePosition, _position, c, 5f);
//                }
//                else if (e.To.OutOfRange)
//                {
//                    Color c = Helper.RandomColour();
////                    Debug.DrawLine(e.To.InRangePosition, _position, c, 5f);
//                    Vector2 newVertexPos = AdvancedMaths.FindLineSegmentCircleIntersections(e.From.Position, e.To.Position, _position, Radius)[0];
//                    e.To.SetInRangePosition(newVertexPos, _position);
//                    c.a = 0.5f;
////                    Debug.DrawLine(e.To.InRangePosition, _position, c, 5f);
//                }
//
//                vertices.Add(e.From);
//                vertices.Add(e.To);
//                edges.Add(e);
//            }));
//            vertices.Sort((a, b) => a.InRangeAngle.CompareTo(b.InRangeAngle));
//
//            for (int i = 0; i < vertices.Count; ++i)
//            {
//                FLVertex nextIndex = Helper.NextElement(i, vertices);
////                Debug.DrawLine(vertices[i].InRangePosition, nextIndex.InRangePosition, Color.red, 5f);
//            }
//
//            for (int i = 0; i < vertices.Count; i++)
//            {
//                FLVertex flVertex = vertices[i];
//                Vector2 dirToVertex = (flVertex.InRangePosition - _position).normalized;
//                Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
//                _intersection = GetIntersectionWithEdge(lineSegmentEnd, edges, flVertex);
//
//                int nextVertexIndex = Helper.NextIndex(i, vertices);
//                FLVertex nextFlVertex = vertices[nextVertexIndex];
//
//                //if there is no intersection check to see if the current vertex is at the end of the mesh, if it is, draw a circular area from this vertex to the next vertex to round off the light
//                if (_intersection.Item1 == false)
//                {
//                    if (flVertex.IsEnd)
//                    {
//                        if (flVertex.OutOfRange) Debug.DrawLine(_position, flVertex.InRangePosition, Color.white, 5f);
//                        if (!nextFlVertex.SharesTransform(flVertex))
//                        {
//                            Vector2 dirToNextVertex = (nextFlVertex.InRangePosition - _position).normalized;
//                            Vector2 nextLineSegmentEnd = _position + dirToNextVertex * Radius;
////                        Debug.DrawLine(_position, lineSegmentEnd, Color.yellow, 5f);
////                        Debug.DrawLine(_position, nextLineSegmentEnd, Color.magenta, 5f);
//                            meshVertices.Add(flVertex.InRangePosition);
//                            meshVertices.Add(lineSegmentEnd);
//                            InsertLineSegments(meshVertices, flVertex.InRangeAngle, nextFlVertex.InRangeAngle);
//                            meshVertices.Add(nextLineSegmentEnd);
//                            meshVertices.Add(nextFlVertex.InRangePosition);
//                        }
//                    }
//                    else
//                    {
//                        meshVertices.Add(flVertex.InRangePosition);
//                    }
//
//                    continue;
//                }
//
//                //if these is an intersection, but it is closer than the current vertex, we can ignore it as it will be dealt with later
//                if (_intersection.Item3 < flVertex.SqrDistanceToOrigin) continue;
//                Vector2 intersectionPoint = _intersection.Item2;
//                if (flVertex.IsStart)
//                {
//                    //start edge intersects with further edge, add further point then closer point
//                    meshVertices.Add(intersectionPoint);
//                    meshVertices.Add(flVertex.InRangePosition);
//                }
//                else if (flVertex.IsEnd)
//                {
//                    //end edge intersects with further edge, add closer point then further point
//                    meshVertices.Add(flVertex.InRangePosition);
//                    float angle = AdvancedMaths.AngleFromUp(_position, intersectionPoint);
//                    if (angle < flVertex.Angle) angle += 360;
//                    if (angle >= nextFlVertex.Angle) meshVertices.Add(intersectionPoint);
//                }
//                else
//                {
//                    //intersection lies behind edge, ignore it
//                    meshVertices.Add(flVertex.InRangePosition);
//                }
//            }
//
//            Mesh mesh = _meshFilter.mesh;
//            mesh.Clear();
//            meshVertices.Insert(0, _position);
//            Vector3[] v = new Vector3[meshVertices.Count];
//            for (int i = 0; i < meshVertices.Count; ++i)
//            {
//                Vector3 vert = transform.InverseTransformPoint(meshVertices[i]);
//                vert.z = 2;
//                v[i] = vert;
//            }
//
//            mesh.vertices = v;
//            mesh.triangles = Triangulate(v);
//            Vector3[] normals = new Vector3[mesh.vertices.Length];
//            for (int i = 0; i < normals.Length; i++) normals[i] = Vector2.up;
//            mesh.normals = normals;
//            mesh.uv = CalculateUvs(mesh.vertices);
//        }

        private int[] Triangulate(Vector3[] vertices)
        {
            List<int> triangles = new List<int>();
            for (int i = 1; i < vertices.Length; ++i)
            {
                int nextTriangle = i + 1 == vertices.Length ? 1 : i + 1;
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(nextTriangle);
            }

            return triangles.ToArray();
        }

        private Vector2[] CalculateUvs(Vector3[] vertices)
        {
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < uvs.Length; i++)
            {
                float u = vertices[i].x / Radius / 2 + 0.5f;
                float v = vertices[i].y / Radius / 2 + 0.5f;

                u = Mathf.Clamp(u, 0f, 1f);
                v = Mathf.Clamp(v, 0f, 1f);

                uvs[i] = new Vector2(u, v);
            }

            return uvs;
        }

        private void InsertLineSegments(List<Vector2> meshVertices, float from, float to)
        {
            float angleIncrement = 1f;
            if (to < from) to += 360;
            for (float angle = from + angleIncrement; angle < to - angleIncrement; angle += angleIncrement)
            {
                float x = _position.x + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                float y = _position.y + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                meshVertices.Add(new Vector2(x, y));
            }
        }

        private void ResizeLight()
        {
            Mesh mesh = _meshFilter.mesh;
            for (int i = 1; i < mesh.vertices.Length; ++i)
            {
                Vector2 vertex = transform.TransformPoint(mesh.vertices[i]);
                Vector2 dir = (_position - vertex).normalized;
                vertex = dir * Radius;
                mesh.vertices[i] = transform.InverseTransformPoint(vertex);
            }
        }

        private bool _hasUpdated;

        private void UpdateLight(Vector2 newPosition)
        {
            bool isPositionSame = newPosition == _position;
            _position = newPosition;
            if (!_needsUpdate && _lastRadius == Radius && isPositionSame) return;
            if (_lastRadius > Radius && isPositionSame)
            {
                ResizeLight();
            }
            else
            {
                DrawLight();
            }

            _needsUpdate = false;
            _hasUpdated = true;
            _lastRadius = Radius;
        }

        private void UpdateColour()
        {
            if (!_hasUpdated && _lastColour == Colour) return;
            Mesh mesh = _meshFilter.mesh;
            Color32[] colors = new Color32[mesh.vertices.Length];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Colour;
            }

            mesh.colors32 = colors;
            _lastColour = Colour;
        }

        public void Update()
        {
            FollowTarget();
            Vector2 newPosition = new Vector2(transform.position.x, transform.position.y);
            UpdateLight(newPosition);
            UpdateColour();
            _hasUpdated = false;
        }

        private void FollowTarget()
        {
            if (Target == null) return;
            transform.position = Target.transform.position;
        }
    }
}