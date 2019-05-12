using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Data
{
	/// <summary>
	/// A queue used to store encroached subsegments.
	/// </summary>
	/// <remarks>
	/// Each subsegment's vertices are stored so that we can check whether a 
	/// subsegment is still the same.
	/// </remarks>
	internal class BadSubseg
	{
		public Vertex org, dest; // Its two vertices.
		public Osub   subseg;    // An encroached subsegment.

		public override int GetHashCode() => subseg.seg.hash;

		public override string ToString() => string.Format("B-SID {0}", subseg.seg.hash);
	}
}