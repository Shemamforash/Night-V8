namespace EpPathFinding.cs
{
	public delegate float HeuristicDelegate(int iDx, int iDy);

	public abstract class ParamBase
	{
		public    DiagonalMovement  DiagonalMovement;
		protected Node              m_endNode;
		protected HeuristicDelegate m_heuristic;

		protected BaseGrid m_searchGrid;
		protected Node     m_startNode;

		public ParamBase(BaseGrid iGrid, GridPos iStartPos, GridPos iEndPos, DiagonalMovement iDiagonalMovement, HeuristicMode iMode) : this(iGrid, iDiagonalMovement, iMode)
		{
			m_startNode = m_searchGrid.GetNodeAt(iStartPos.x, iStartPos.y);
			m_endNode   = m_searchGrid.GetNodeAt(iEndPos.x,   iEndPos.y);
			if (m_startNode == null)
			{
				m_startNode = new Node(iStartPos.x, iStartPos.y, true);
			}

			if (m_endNode == null)
			{
				m_endNode = new Node(iEndPos.x, iEndPos.y, true);
			}
		}

		public ParamBase(BaseGrid iGrid, DiagonalMovement iDiagonalMovement, HeuristicMode iMode)
		{
			SetHeuristic(iMode);

			m_searchGrid     = iGrid;
			DiagonalMovement = iDiagonalMovement;
			m_startNode      = null;
			m_endNode        = null;
		}

		public ParamBase(ParamBase param)
		{
			m_searchGrid     = param.m_searchGrid;
			DiagonalMovement = param.DiagonalMovement;
			m_startNode      = param.m_startNode;
			m_endNode        = param.m_endNode;
		}

		public HeuristicDelegate HeuristicFunc => m_heuristic;

		public BaseGrid SearchGrid => m_searchGrid;

		public Node StartNode => m_startNode;

		public Node EndNode => m_endNode;

		internal abstract void _reset(GridPos iStartPos, GridPos iEndPos, BaseGrid iSearchGrid = null);

		public void Reset(GridPos iStartPos, GridPos iEndPos, BaseGrid iSearchGrid = null)
		{
			_reset(iStartPos, iEndPos, iSearchGrid);
			m_startNode = null;
			m_endNode   = null;

			if (iSearchGrid != null)
			{
				m_searchGrid = iSearchGrid;
			}

			m_searchGrid.Reset();
			m_startNode = m_searchGrid.GetNodeAt(iStartPos.x, iStartPos.y);
			m_endNode   = m_searchGrid.GetNodeAt(iEndPos.x,   iEndPos.y);
			if (m_startNode == null)
			{
				m_startNode = new Node(iStartPos.x, iStartPos.y, true);
			}

			if (m_endNode == null)
			{
				m_endNode = new Node(iEndPos.x, iEndPos.y, true);
			}
		}

		public void SetHeuristic(HeuristicMode iMode)
		{
			m_heuristic = null;
			switch (iMode)
			{
				case HeuristicMode.MANHATTAN:
					m_heuristic = Heuristic.Manhattan;
					break;
				case HeuristicMode.EUCLIDEAN:
					m_heuristic = Heuristic.Euclidean;
					break;
				case HeuristicMode.CHEBYSHEV:
					m_heuristic = Heuristic.Chebyshev;
					break;
				default:
					m_heuristic = Heuristic.Euclidean;
					break;
			}
		}
	}
}