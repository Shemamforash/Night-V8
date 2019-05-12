using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Data
{
	/// <summary>
	/// A queue used to store bad triangles.
	/// </summary>
	/// <remarks>
	/// The key is the square of the cosine of the smallest angle of the triangle.
	/// Each triangle's vertices are stored so that one can check whether a
	/// triangle is still the same.
	/// </remarks>
	internal class BadTriangle
	{
		public double      key;             // cos^2 of smallest (apical) angle.
		public BadTriangle next;            // Pointer to next bad triangle.
		public Vertex      org, dest, apex; // Its three vertices.
		public Otri        poortri;         // A skinny or too-large triangle.

		public override string ToString() => string.Format("B-TID {0}", poortri.tri.hash);
	}
}