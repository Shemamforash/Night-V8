using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Voronoi.Legacy
{
	/// <summary>
	/// Represents a region in the Voronoi diagram.
	/// </summary>
	public class VoronoiRegion
	{
		private          bool  bounded;
		private readonly Point generator;
		private readonly int   id;

		// A map (vertex id) -> (neighbor across adjacent edge)
		private readonly Dictionary<int, VoronoiRegion> neighbors;
		private readonly List<Point>                    vertices;

		public VoronoiRegion(Vertex generator)
		{
			id             = generator.id;
			this.generator = generator;
			vertices       = new List<Point>();
			bounded        = true;

			neighbors = new Dictionary<int, VoronoiRegion>();
		}

		/// <summary>
		/// Gets the Voronoi region id (which is the same as the generators vertex id).
		/// </summary>
		public int ID => id;

		/// <summary>
		/// Gets the Voronoi regions generator.
		/// </summary>
		public Point Generator => generator;

		/// <summary>
		/// Gets the Voronoi vertices on the regions boundary.
		/// </summary>
		public ICollection<Point> Vertices => vertices;

		/// <summary>
		/// Gets or sets whether the Voronoi region is bounded.
		/// </summary>
		public bool Bounded
		{
			get => bounded;
			set => bounded = value;
		}

		public void Add(Point point)
		{
			vertices.Add(point);
		}

		public void Add(List<Point> points)
		{
			vertices.AddRange(points);
		}

		/// <summary>
		/// Returns the neighbouring Voronoi region, that lies across the edge starting at
		/// given vertex.
		/// </summary>
		/// <param name="p">Vertex defining an edge of the region.</param>
		/// <returns>Neighbouring Voronoi region</returns>
		/// <remarks>
		/// The edge starting at p is well defined (vertices are ordered counterclockwise).
		/// </remarks>
		public VoronoiRegion GetNeighbor(Point p)
		{
			VoronoiRegion neighbor;

			if (neighbors.TryGetValue(p.id, out neighbor))
			{
				return neighbor;
			}

			return null;
		}

		internal void AddNeighbor(int id, VoronoiRegion neighbor)
		{
			neighbors.Add(id, neighbor);
		}

		public override string ToString() => string.Format("R-ID {0}", id);
	}
}