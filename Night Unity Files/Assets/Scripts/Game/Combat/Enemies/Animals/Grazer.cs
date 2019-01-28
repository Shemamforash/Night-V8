using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        public override void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            Cell target = WorldGrid.GetEdgeCell(transform.position);
            MoveBehaviour.GoToCell(target);
        }
    }
}