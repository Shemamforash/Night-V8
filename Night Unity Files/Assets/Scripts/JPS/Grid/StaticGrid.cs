using System;

namespace EpPathFinding.cs
{
	public class StaticGrid : BaseGrid
	{
		private Node[][] m_nodes;

		public StaticGrid(int iWidth, int iHeight, bool[][] iMatrix = null)
		{
			width           = iWidth;
			height          = iHeight;
			m_gridRect.minX = 0;
			m_gridRect.minY = 0;
			m_gridRect.maxX = iWidth  - 1;
			m_gridRect.maxY = iHeight - 1;
			m_nodes         = buildNodes(iWidth, iHeight, iMatrix);
		}

		public StaticGrid(StaticGrid b)
			: base(b)
		{
			bool[][] tMatrix = new bool[b.width][];
			for (int widthTrav = 0; widthTrav < b.width; widthTrav++)
			{
				tMatrix[widthTrav] = new bool[b.height];
				for (int heightTrav = 0; heightTrav < b.height; heightTrav++)
				{
					if (b.IsWalkableAt(widthTrav, heightTrav))
					{
						tMatrix[widthTrav][heightTrav] = true;
					}
					else
					{
						tMatrix[widthTrav][heightTrav] = false;
					}
				}
			}

			m_nodes = buildNodes(b.width, b.height, tMatrix);
		}

		private Node[][] buildNodes(int iWidth, int iHeight, bool[][] iMatrix)
		{
			Node[][] tNodes = new Node[iWidth][];
			for (int widthTrav = 0; widthTrav < iWidth; widthTrav++)
			{
				tNodes[widthTrav] = new Node[iHeight];
				for (int heightTrav = 0; heightTrav < iHeight; heightTrav++) tNodes[widthTrav][heightTrav] = new Node(widthTrav, heightTrav);
			}

			if (iMatrix == null)
			{
				return tNodes;
			}

			if (iMatrix.Length != iWidth || iMatrix[0].Length != iHeight)
			{
				throw new Exception("Matrix size does not fit");
			}


			for (int widthTrav = 0; widthTrav < iWidth; widthTrav++)
			{
				for (int heightTrav = 0; heightTrav < iHeight; heightTrav++)
				{
					if (iMatrix[widthTrav][heightTrav])
					{
						tNodes[widthTrav][heightTrav].walkable = true;
					}
					else
					{
						tNodes[widthTrav][heightTrav].walkable = false;
					}
				}
			}

			return tNodes;
		}

		public override Node GetNodeAt(int iX, int iY) => m_nodes[iX][iY];

		public override bool IsWalkableAt(int iX, int iY) => isInside(iX, iY) && m_nodes[iX][iY].walkable;

		public bool isInside(int iX, int iY) => iX >= 0 && iX < width && iY >= 0 && iY < height;

		public override bool SetWalkableAt(int iX, int iY, bool iWalkable)
		{
			m_nodes[iX][iY].walkable = iWalkable;
			return true;
		}

		protected bool isInside(GridPos iPos) => isInside(iPos.x, iPos.y);

		public override Node GetNodeAt(GridPos iPos) => GetNodeAt(iPos.x, iPos.y);

		public override bool IsWalkableAt(GridPos iPos) => IsWalkableAt(iPos.x, iPos.y);

		public override bool SetWalkableAt(GridPos iPos, bool iWalkable) => SetWalkableAt(iPos.x, iPos.y, iWalkable);

		public override void Reset()
		{
			Reset(null);
		}

		public void Reset(bool[][] iMatrix)
		{
			for (int widthTrav = 0; widthTrav < width; widthTrav++)
			{
				for (int heightTrav = 0; heightTrav < height; heightTrav++) m_nodes[widthTrav][heightTrav].Reset();
			}

			if (iMatrix == null)
			{
				return;
			}

			if (iMatrix.Length != width || iMatrix[0].Length != height)
			{
				throw new Exception("Matrix size does not fit");
			}

			for (int widthTrav = 0; widthTrav < width; widthTrav++)
			{
				for (int heightTrav = 0; heightTrav < height; heightTrav++)
				{
					if (iMatrix[widthTrav][heightTrav])
					{
						m_nodes[widthTrav][heightTrav].walkable = true;
					}
					else
					{
						m_nodes[widthTrav][heightTrav].walkable = false;
					}
				}
			}
		}

		public override BaseGrid Clone()
		{
			int      tWidth  = width;
			int      tHeight = height;
			Node[][] tNodes  = m_nodes;

			StaticGrid tNewGrid = new StaticGrid(tWidth, tHeight);

			Node[][] tNewNodes = new Node[tWidth][];
			for (int widthTrav = 0; widthTrav < tWidth; widthTrav++)
			{
				tNewNodes[widthTrav] = new Node[tHeight];
				for (int heightTrav = 0; heightTrav < tHeight; heightTrav++) tNewNodes[widthTrav][heightTrav] = new Node(widthTrav, heightTrav, tNodes[widthTrav][heightTrav].walkable);
			}

			tNewGrid.m_nodes = tNewNodes;

			return tNewGrid;
		}
	}
}