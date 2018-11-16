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
        
        private void WaitForPlayer()
        {
            if (Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position) > 2) return;
            Vector2 dir = (transform.position - PlayerCombat.Instance.transform.position).normalized;
            Cell target = PathingGrid.GetCellsInFrontOfMe(CurrentCell(), dir, 0.5f).RandomElement();
            if (target == null) target = PathingGrid.GetCellNearMe(CurrentCell(), 1f, 0.5f);
            MoveBehaviour.GoToCell(target);
            CurrentAction = WaitForPlayer;
        }
    }
}