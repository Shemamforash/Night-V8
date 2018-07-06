using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Curio : AnimalBehaviour
    {
        protected override void OnAlert()
        {
            ChooseNextAction();
        }

        public override void ChooseNextAction()
        {
            Vector2 dir = (transform.position - PlayerCombat.Instance.transform.position).normalized;
            Cell target = Helper.RandomInList(PathingGrid.GetCellsInFrontOfMe(CurrentCell(), dir, 1));
            GetRouteToCell(target, WaitForPlayer);
        }

        private void WaitForPlayer()
        {
            if (Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position) > 2) return;
            ChooseNextAction();
        }
    }
}