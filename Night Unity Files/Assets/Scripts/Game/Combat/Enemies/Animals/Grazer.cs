using Game.Combat.Enemies.Nightmares;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        private EnemyBehaviour _leader;

        public void Initialise(Enemy e)
        {
            base.Initialise(e);
            WanderDistance = 1f;
        }

        protected override void OnAlert()
        {
            Flee();
        }

        protected override void Flee()
        {
            if (_leader == null)
            {
                base.Flee();
                return;
            }

            MoveToCharacter(_leader, Flee);
        }

        protected override void CheckForPlayer()
        {
        }

        public void SetLeader(EnemyBehaviour leader)
        {
            _leader = leader;
        }
    }
}