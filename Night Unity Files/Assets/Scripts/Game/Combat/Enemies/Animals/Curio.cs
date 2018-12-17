using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Curio : AnimalBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            CurrentAction = WaitForPlayer;
        }

        public override void Alert(bool alertOthers)
        {
            Move();
        }

        private void Move()
        {
            Vector2 dir = (transform.position - PlayerCombat.Instance.transform.position).normalized;
            float distance = Random.Range(0.5f, 3f);
            List<Cell> possibleCells = PathingGrid.GetCellsInFrontOfMe(CurrentCell(), dir, distance);
            Cell target = possibleCells.Count == 0 ? PathingGrid.GetCellNearMe(CurrentCell(), distance * 1.5f, distance) : possibleCells.RandomElement();
            MoveBehaviour.GoToCell(target);
        }

        private void WaitForPlayer()
        {
            if (Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position) > 2) return;
            Move();
            CurrentAction = WaitForPlayer;
        }
    }
}