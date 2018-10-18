namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Beam : TimedAttackBehaviour
    {
        private BeamController _beamController;

        protected override void Attack()
        {
            _beamController = BeamController.Create(transform);
        }

        private void OnDestroy()
        {
            if (_beamController == null) return;
            Destroy(_beamController.gameObject);
        }
    }
}