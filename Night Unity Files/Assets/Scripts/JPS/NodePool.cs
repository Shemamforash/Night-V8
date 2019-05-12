using System.Collections.Generic;

namespace EpPathFinding.cs
{
	public class NodePool
	{
		protected Dictionary<GridPos, Node> m_nodes;

		public NodePool() => m_nodes = new Dictionary<GridPos, Node>();

		public Dictionary<GridPos, Node> Nodes => m_nodes;

		public Node GetNode(int iX, int iY)
		{
			GridPos pos = new GridPos(iX, iY);
			return GetNode(pos);
		}

		public Node GetNode(GridPos iPos)
		{
			Node retVal = null;
			m_nodes.TryGetValue(iPos, out retVal);
			return retVal;
		}

		public Node SetNode(int iX, int iY, bool? iWalkable = null)
		{
			GridPos pos = new GridPos(iX, iY);
			return SetNode(pos, iWalkable);
		}

		public Node SetNode(GridPos iPos, bool? iWalkable = null)
		{
			if (iWalkable.HasValue)
			{
				if (iWalkable.Value)
				{
					Node retVal = null;
					if (m_nodes.TryGetValue(iPos, out retVal))
					{
						return retVal;
					}

					Node newNode = new Node(iPos.x, iPos.y, iWalkable);
					m_nodes.Add(iPos, newNode);
					return newNode;
				}

				removeNode(iPos);
			}
			else
			{
				Node newNode = new Node(iPos.x, iPos.y, true);
				m_nodes.Add(iPos, newNode);
				return newNode;
			}

			return null;
		}

		protected void removeNode(int iX, int iY)
		{
			GridPos pos = new GridPos(iX, iY);
			removeNode(pos);
		}

		protected void removeNode(GridPos iPos)
		{
			if (m_nodes.ContainsKey(iPos))
			{
				m_nodes.Remove(iPos);
			}
		}
	}
}