using Game.Combat.Enemies.Nightmares;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        protected override void OnAlert()
        {
            Flee();
        }

        protected override void CheckForPlayer()
        {
            
        }
    }
}