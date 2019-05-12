using TriangleNet.Geometry;

namespace TriangleNet.Topology
{
	/// <summary>
	/// The subsegment data structure.
	/// </summary>
	public class SubSegment : ISegment
	{
		internal int boundary;

		// Hash for dictionary. Will be set by mesh instance.
		internal int hash;

		internal Osub[]   subsegs;
		internal Otri[]   triangles;
		internal Vertex[] vertices;

		public SubSegment()
		{
			// Four NULL vertices.
			vertices = new Vertex[4];

			// Set the boundary marker to zero.
			boundary = 0;

			// Initialize the two adjoining subsegments to be the omnipresent
			// subsegment.
			subsegs = new Osub[2];

			// Initialize the two adjoining triangles to be "outer space."
			triangles = new Otri[2];
		}

		/// <summary>
		/// Gets the segments endpoint.
		/// </summary>
		public Vertex GetVertex(int index) => vertices[index];

		/// <summary>
		/// Gets an adjoining triangle.
		/// </summary>
		public ITriangle GetTriangle(int index) => triangles[index].tri.hash == Mesh.DUMMY ? null : triangles[index].tri;

		public override int GetHashCode() => hash;

		public override string ToString() => string.Format("SID {0}", hash);

		#region Public properties

		/// <summary>
		/// Gets the first endpoints vertex id.
		/// </summary>
		public int P0 => vertices[0].id;

		/// <summary>
		/// Gets the seconds endpoints vertex id.
		/// </summary>
		public int P1 => vertices[1].id;

		/// <summary>
		/// Gets the segment boundary mark.
		/// </summary>
		public int Label => boundary;

		#endregion
	}
}