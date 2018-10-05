namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Beam : TimedAttackBehaviour
    {
        private BeamController _beamController;

        protected override void Attack()
        {
            SkillAnimationController.Create(transform, "Beam", 1f, () =>
            {
                _beamController = BeamController.Create(transform);
            });
        }

        private void OnDestroy()
        {
            if (_beamController == null) return;
            Destroy(_beamController.gameObject);
        }
    }
}