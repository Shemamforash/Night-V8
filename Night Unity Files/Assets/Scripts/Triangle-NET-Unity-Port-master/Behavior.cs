using System;
using TriangleNet.Geometry;

namespace TriangleNet
{
	/// <summary>
	/// Controls the behavior of the meshing software.
	/// </summary>
	internal class Behavior
	{
		private bool boundaryMarkers = true;
		private bool conformDel;
		private bool convex;

		internal bool   fixedArea;
		internal double goodAngle;
		private  bool   jettison;
		private  double maxAngle;
		private  double maxArea = -1.0;
		internal double maxGoodAngle;

		private double minAngle;

		private  int    noBisect;
		private  bool   noHoles;
		internal double offconstant;
		private  bool   poly;
		private  bool   quality;
		internal bool   useRegions = false;

		private  Func<ITriangle, double, bool> usertest;
		internal bool                          useSegments = true;
		private  bool                          varArea;

		/// <summary>
		/// Creates an instance of the Behavior class.
		/// </summary>
		public Behavior(bool quality = false, double minAngle = 20.0)
		{
			if (quality)
			{
				this.quality  = true;
				this.minAngle = minAngle;

				Update();
			}
		}

		#region Static properties

		/// <summary>
		/// No exact arithmetic.
		/// </summary>
		public static bool NoExact { get; set; }

		#endregion

		/// <summary>
		/// Update quality options dependencies.
		/// </summary>
		private void Update()
		{
			quality = true;

			if (minAngle < 0 || minAngle > 60)
			{
				minAngle = 0;
				quality  = false;

				Log.Instance.Warning("Invalid quality option (minimum angle).", "Mesh.Behavior");
			}

			if (maxAngle != 0.0 && (maxAngle < 60 || maxAngle > 180))
			{
				maxAngle = 0;
				quality  = false;

				Log.Instance.Warning("Invalid quality option (maximum angle).", "Mesh.Behavior");
			}

			useSegments  = Poly || Quality || Convex;
			goodAngle    = Math.Cos(MinAngle * Math.PI / 180.0);
			maxGoodAngle = Math.Cos(MaxAngle * Math.PI / 180.0);

			if (goodAngle == 1.0)
			{
				offconstant = 0.0;
			}
			else
			{
				offconstant = 0.475 * Math.Sqrt((1.0 + goodAngle) / (1.0 - goodAngle));
			}

			goodAngle *= goodAngle;
		}

		#region Public properties

		/// <summary>
		/// Quality mesh generation.
		/// </summary>
		public bool Quality
		{
			get => quality;
			set
			{
				quality = value;
				if (quality)
				{
					Update();
				}
			}
		}

		/// <summary>
		/// Minimum angle constraint.
		/// </summary>
		public double MinAngle
		{
			get => minAngle;
			set
			{
				minAngle = value;
				Update();
			}
		}

		/// <summary>
		/// Maximum angle constraint.
		/// </summary>
		public double MaxAngle
		{
			get => maxAngle;
			set
			{
				maxAngle = value;
				Update();
			}
		}

		/// <summary>
		/// Maximum area constraint.
		/// </summary>
		public double MaxArea
		{
			get => maxArea;
			set
			{
				maxArea   = value;
				fixedArea = value > 0.0;
			}
		}

		/// <summary>
		/// Apply a maximum triangle area constraint.
		/// </summary>
		public bool VarArea
		{
			get => varArea;
			set => varArea = value;
		}

		/// <summary>
		/// Input is a Planar Straight Line Graph.
		/// </summary>
		public bool Poly
		{
			get => poly;
			set => poly = value;
		}

		/// <summary>
		/// Apply a user-defined triangle constraint.
		/// </summary>
		public Func<ITriangle, double, bool> UserTest
		{
			get => usertest;
			set => usertest = value;
		}

		/// <summary>
		/// Enclose the convex hull with segments.
		/// </summary>
		public bool Convex
		{
			get => convex;
			set => convex = value;
		}

		/// <summary>
		/// Conforming Delaunay (all triangles are truly Delaunay).
		/// </summary>
		public bool ConformingDelaunay
		{
			get => conformDel;
			set => conformDel = value;
		}

		/// <summary>
		/// Suppresses boundary segment splitting.
		/// </summary>
		/// <remarks>
		/// 0 = split segments
		/// 1 = no new vertices on the boundary
		/// 2 = prevent all segment splitting, including internal boundaries
		/// </remarks>
		public int NoBisect
		{
			get => noBisect;
			set
			{
				noBisect = value;
				if (noBisect < 0 || noBisect > 2)
				{
					noBisect = 0;
				}
			}
		}

		/// <summary>
		/// Compute boundary information.
		/// </summary>
		public bool UseBoundaryMarkers
		{
			get => boundaryMarkers;
			set => boundaryMarkers = value;
		}

		/// <summary>
		/// Ignores holes in polygons.
		/// </summary>
		public bool NoHoles
		{
			get => noHoles;
			set => noHoles = value;
		}

		/// <summary>
		/// Jettison unused vertices from output.
		/// </summary>
		public bool Jettison
		{
			get => jettison;
			set => jettison = value;
		}

		#endregion
	}
}