using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;

namespace Game.Combat.Enemies
{
	public class UnarmedBehaviour : EnemyBehaviour
	{
		private   Cell  _lastTargetCell;
		private   bool  _wandering;
		protected float MaxDistance, MinDistance;
		protected Cell  TargetCell;

		public override void Initialise(Enemy e)
		{
			base.Initialise(e);
		}

		private void GoToTargetCell()
		{
			if (TargetCell == _lastTargetCell) return;
			MoveBehaviour.GoToCell(TargetCell, MaxDistance, MinDistance);
			_lastTargetCell = TargetCell;
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			UpdateTargetCell();
			GoToTargetCell();
		}

		protected virtual void UpdateTargetCell()
		{
			if (GetTarget() == null) SetTarget(PlayerCombat.Instance);
			if (GetTarget() == null) return;
			Cell newTargetCell = ((CharacterCombat) GetTarget()).CurrentCell();
			if (!newTargetCell.Reachable)
			{
				List<Cell> cellsNearMe = WorldGrid.GetCellsNearMe(newTargetCell.Position, 1, 1);
				if (cellsNearMe.Count == 0) return;
				TargetCell = cellsNearMe[0];
				return;
			}

			TargetCell = newTargetCell;
		}
	}
}