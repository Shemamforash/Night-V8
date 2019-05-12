using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	/// <summary>
	/// Simple triangle class for input.
	/// </summary>
	public class InputTriangle : ITriangle
	{
		internal double area;
		internal int    label;
		internal int[]  vertices;

		public InputTriangle(int p0, int p1, int p2) => vertices = new[] {p0, p1, p2};

		#region Public properties

		/// <summary>
		/// Gets the triangle id.
		/// </summary>
		public int ID
		{
			get => 0;
			set { }
		}

		/// <summary>
		/// Region ID the triangle belongs to.
		/// </summary>
		public int Label
		{
			get => label;
			set => label = value;
		}

		/// <summary>
		/// Gets the triangle area constraint.
		/// </summary>
		public double Area
		{
			get => area;
			set => area = value;
		}

		/// <summary>
		/// Gets the specified corners vertex.
		/// </summary>
		public Vertex GetVertex(int index) => null;

		public int GetVertexID(int index) => vertices[index];

		public ITriangle GetNeighbor(int index) => null;

		public int GetNeighborID(int index) => -1;

		public ISegment GetSegment(int index) => null;

		#endregion
	}
}