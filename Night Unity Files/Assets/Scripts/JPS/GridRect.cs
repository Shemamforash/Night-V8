namespace EpPathFinding.cs
{
	public class GridRect
	{
		public int maxX;
		public int maxY;
		public int minX;
		public int minY;

		public GridRect()
		{
			minX = 0;
			minY = 0;
			maxX = 0;
			maxY = 0;
		}

		public GridRect(int iMinX, int iMinY, int iMaxX, int iMaxY)
		{
			minX = iMinX;
			minY = iMinY;
			maxX = iMaxX;
			maxY = iMaxY;
		}

		public GridRect(GridRect b)
		{
			minX = b.minX;
			minY = b.minY;
			maxX = b.maxX;
			maxY = b.maxY;
		}

		public override int GetHashCode() => minX ^ minY ^ maxX ^ maxY;

		public override bool Equals(object obj)
		{
			// Unlikely to compare incorrect type so removed for performance
			//if (!(obj.GetType() == typeof(GridRect)))
			//    return false;
			GridRect p = (GridRect) obj;
			if (ReferenceEquals(null, p))
			{
				return false;
			}

			// Return true if the fields match:
			return minX == p.minX && minY == p.minY && maxX == p.maxX && maxY == p.maxY;
		}

		public bool Equals(GridRect p)
		{
			if (ReferenceEquals(null, p))
			{
				return false;
			}

			// Return true if the fields match:
			return minX == p.minX && minY == p.minY && maxX == p.maxX && maxY == p.maxY;
		}

		public static bool operator ==(GridRect a, GridRect b)
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
			return a.minX == b.minX && a.minY == b.minY && a.maxX == b.maxX && a.maxY == b.maxY;
		}

		public static bool operator !=(GridRect a, GridRect b) => !(a == b);

		public GridRect Set(int iMinX, int iMinY, int iMaxX, int iMaxY)
		{
			minX = iMinX;
			minY = iMinY;
			maxX = iMaxX;
			maxY = iMaxY;
			return this;
		}
	}
}