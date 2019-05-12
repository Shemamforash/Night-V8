using TriangleNet.Meshing;

namespace TriangleNet.Smoothing
{
	/// <summary>
	/// Interface for mesh smoothers.
	/// </summary>
	public interface ISmoother
	{
		void Smooth(IMesh mesh);
		void Smooth(IMesh mesh, int limit);
	}
}