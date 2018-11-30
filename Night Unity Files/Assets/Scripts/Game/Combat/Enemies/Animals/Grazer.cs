using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        public override void Alert(bool alertOthers)
        {
            if (Alerted) return;
            Alerted = true;
            Cell target = PathingGrid.GetCellOutOfRange(transform.position);
            MoveBehaviour.GoToCell(target);
        }
    }
}