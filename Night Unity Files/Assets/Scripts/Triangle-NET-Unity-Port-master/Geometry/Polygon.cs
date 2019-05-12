using System;
using System.Collections.Generic;

namespace TriangleNet.Geometry
{
	/// <summary>
	/// A polygon represented as a planar straight line graph.
	/// </summary>
	public class Polygon : IPolygon
	{
		private readonly List<Point>         holes;
		private readonly List<Vertex>        points;
		private readonly List<RegionPointer> regions;

		private readonly List<ISegment> segments;

		/// <summary>
		/// Initializes a new instance of the <see cref="Polygon" /> class.
		/// </summary>
		public Polygon()
			: this(3, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Polygon" /> class.
		/// </summary>
		/// <param name="capacity">The default capacity for the points list.</param>
		public Polygon(int capacity)
			: this(3, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Polygon" /> class.
		/// </summary>
		/// <param name="capacity">The default capacity for the points list.</param>
		/// <param name="markers">Use point and segment markers.</param>
		public Polygon(int capacity, bool markers)
		{
			points  = new List<Vertex>(capacity);
			holes   = new List<Point>();
			regions = new List<RegionPointer>();

			segments = new List<ISegment>();

			HasPointMarkers   = markers;
			HasSegmentMarkers = markers;
		}

		/// <inheritdoc />
		public int Count => points.Count;

		/// <inheritdoc />
		public List<Vertex> Points => points;

		/// <inheritdoc />
		public List<Point> Holes => holes;

		/// <inheritdoc />
		public List<RegionPointer> Regions => regions;

		/// <inheritdoc />
		public List<ISegment> Segments => segments;

		/// <inheritdoc />
		public bool HasPointMarkers { get; set; }

		/// <inheritdoc />
		public bool HasSegmentMarkers { get; set; }

		[Obsolete("Use polygon.Add(contour) method instead.")]
		public void AddContour(IEnumerable<Vertex> points,       int  marker = 0,
		                       bool                hole = false, bool convex = false)
		{
			Add(new Contour(points, marker, convex), hole);
		}

		[Obsolete("Use polygon.Add(contour) method instead.")]
		public void AddContour(IEnumerable<Vertex> points, int marker, Point hole)
		{
			Add(new Contour(points, marker), hole);
		}

		/// <inheritdoc />
		public Rectangle Bounds()
		{
			var bounds = new Rectangle();
			bounds.Expand(points);

			return bounds;
		}

		/// <summary>
		/// Add a vertex to the polygon.
		/// </summary>
		/// <param name="vertex">The vertex to insert.</param>
		public void Add(Vertex vertex)
		{
			points.Add(vertex);
		}

		/// <summary>
		/// Add a segment to the polygon.
		/// </summary>
		/// <param name="segment">The segment to insert.</param>
		/// <param name="insert">If true, both endpoints will be added to the points list.</param>
		public void Add(ISegment segment, bool insert = false)
		{
			segments.Add(segment);

			if (insert)
			{
				points.Add(segment.GetVertex(0));
				points.Add(segment.GetVertex(1));
			}
		}

		/// <summary>
		/// Add a segment to the polygon.
		/// </summary>
		/// <param name="segment">The segment to insert.</param>
		/// <param name="index">The index of the segment endpoint to add to the points list (must be 0 or 1).</param>
		public void Add(ISegment segment, int index)
		{
			segments.Add(segment);

			points.Add(segment.GetVertex(index));
		}

		/// <summary>
		/// Add a contour to the polygon.
		/// </summary>
		/// <param name="contour">The contour to insert.</param>
		/// <param name="hole">Treat contour as a hole.</param>
		public void Add(Contour contour, bool hole = false)
		{
			if (hole)
			{
				Add(contour, contour.FindInteriorPoint());
			}
			else
			{
				points.AddRange(contour.Points);
				segments.AddRange(contour.GetSegments());
			}
		}

		/// <summary>
		/// Add a contour to the polygon.
		/// </summary>
		/// <param name="contour">The contour to insert.</param>
		/// <param name="hole">Point inside the contour, making it a hole.</param>
		public void Add(Contour contour, Point hole)
		{
			points.AddRange(contour.Points);
			segments.AddRange(contour.GetSegments());

			holes.Add(hole);
		}
	}
}