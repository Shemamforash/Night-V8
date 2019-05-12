using System;

namespace EpPathFinding.cs
{
	public class GridPos : IEquatable<GridPos>
	{
		public int x;
		public int y;

		public GridPos()
		{
			x = 0;
			y = 0;
		}

		public GridPos(int iX, int iY)
		{
			x = iX;
			y = iY;
		}

		public GridPos(GridPos b)
		{
			x = b.x;
			y = b.y;
		}

		public bool Equals(GridPos p)
		{
			if (ReferenceEquals(null, p))
			{
				return false;
			}

			// Return true if the fields match:
			return x == p.x && y == p.y;
		}

		public override int GetHashCode() => x ^ y;

		public override bool Equals(object obj)
		{
			// Unlikely to compare incorrect type so removed for performance
			// if (!(obj.GetType() == typeof(GridPos)))
			//     return false;
			GridPos p = (GridPos) obj;

			if (ReferenceEquals(null, p))
			{
				return false;
			}

			// Return true if the fields match:
			return x == p.x && y == p.y;
		}

		public static bool operator ==(GridPos a, GridPos b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (ReferenceEquals(null, a))
			{
				return false;
			}

			if (ReferenceEquals(null, b))
			{
				return false;
			}

			// Return true if the fields match:
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(GridPos a, GridPos b) => !(a == b);

		public GridPos Set(int iX, int iY)
		{
			x = iX;
			y = iY;
			return this;
		}

		public override string ToString() => string.Format("({0},{1})", x, y);
	}
}