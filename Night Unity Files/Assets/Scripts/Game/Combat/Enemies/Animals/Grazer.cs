using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Misc;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        private readonly List<Grazer> _herd = new List<Grazer>();

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
            _herd.ForEach(g => g.Flee(target));
        }

        protected override void CheckForPlayer()
        {
        }

        public void AddHerdMembers(List<EnemyBehaviour> herd)
        {
            herd.ForEach(e =>
            {
                Grazer g = e as Grazer;
                if (g == null || g == this) return;
                _herd.Add(g);
            });
        }
    }
}