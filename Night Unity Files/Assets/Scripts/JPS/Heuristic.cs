using System;

namespace EpPathFinding.cs
{
	public enum HeuristicMode
	{
		MANHATTAN,
		EUCLIDEAN,
		CHEBYSHEV
	}

	public class Heuristic
	{
		public static float Manhattan(int iDx, int iDy) => (float) iDx + iDy;

		public static float Euclidean(int iDx, int iDy)
		{
			float tFdx = iDx;
			float tFdy = iDy;
			return (float) Math.Sqrt(tFdx * tFdx + tFdy * tFdy);
		}

		public static float Chebyshev(int iDx, int iDy) => Math.Max(iDx, iDy);
	}
}