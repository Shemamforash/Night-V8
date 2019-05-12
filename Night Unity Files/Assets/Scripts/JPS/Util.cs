namespace EpPathFinding.cs
{
	public class Util
	{
		public static DiagonalMovement GetDiagonalMovement(bool iCrossCorners, bool iCrossAdjacentPoint)
		{
			if (iCrossCorners && iCrossAdjacentPoint)
			{
				return DiagonalMovement.Always;
			}

			if (iCrossCorners)
			{
				return DiagonalMovement.IfAtLeastOneWalkable;
			}

			return DiagonalMovement.OnlyWhenNoObstacles;
		}
	}
}