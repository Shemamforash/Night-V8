using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamsHelper.Libraries
{
	public class Polygon
	{
		public readonly List<Vector2> Vertices;
		public          Vector2       Position;
		public          Vector2       TopLeft,  BottomRight;
		private         Vector2       TopRight, BottomLeft;

		public Polygon(List<Vector2> vertices, Vector2 position)
		{
			Vertices = vertices;
			Position = position;
			SetBoundingCorners();
		}

		public bool IsVisible() => AdvancedMaths.IsRectInCameraView(TopLeft, TopRight, BottomRight, BottomLeft);

		private void SetBoundingCorners()
		{
			if (Vertices.Count < 4) return;
			Tuple<Vector3, Vector3> boundingCorners = AdvancedMaths.GetBoundingCornersOfPolygon(Vertices);
			TopLeft     =  boundingCorners.Item1;
			BottomRight =  boundingCorners.Item2;
			TopLeft     += Position;
			BottomRight += Position;
			TopRight    =  new Vector2(BottomRight.x, TopLeft.y);
			BottomLeft  =  new Vector2(TopLeft.x,     BottomRight.y);
		}

		public void DrawBounds(float duration = 1f)
		{
			Color c = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);
			Debug.DrawLine(TopLeft,     TopRight,   c, duration);
			Debug.DrawLine(TopLeft,     BottomLeft, c, duration);
			Debug.DrawLine(BottomRight, TopRight,   c, duration);
			Debug.DrawLine(BottomRight, BottomLeft, c, duration);
		}
	}
}