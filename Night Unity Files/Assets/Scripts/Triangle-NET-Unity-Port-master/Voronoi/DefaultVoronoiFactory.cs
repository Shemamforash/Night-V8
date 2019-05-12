﻿using TriangleNet.Topology.DCEL;

namespace TriangleNet.Voronoi
{
	/// <summary>
	/// Default factory for Voronoi / DCEL mesh objects.
	/// </summary>
	public class DefaultVoronoiFactory : IVoronoiFactory
	{
		public void Initialize(int vertexCount, int edgeCount, int faceCount)
		{
		}

		public void Reset()
		{
		}

		public Vertex CreateVertex(double x, double y) => new Vertex(x, y);

		public HalfEdge CreateHalfEdge(Vertex origin, Face face) => new HalfEdge(origin, face);

		public Face CreateFace(Geometry.Vertex vertex) => new Face(vertex);
	}
}