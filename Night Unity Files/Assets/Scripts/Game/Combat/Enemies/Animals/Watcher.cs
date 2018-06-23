using Game.Combat.Enemies.Nightmares;

namespace Game.Combat.Enemies.Animals
{
    public class Watcher : AnimalBehaviour
    {
        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            AlertAll = true;
        }
    }
}