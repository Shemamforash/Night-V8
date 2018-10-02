using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Misc;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            WanderDistance = 1f;
        }
        
        public override void TakeShotDamage(Shot shot)
        {
            base.TakeShotDamage(shot);
            if (Fleeing) return;
            Cell target = PathingGrid.GetCellOutOfRange(transform.position);
            Flee(target);
            Alert(true);
        }
    }
}