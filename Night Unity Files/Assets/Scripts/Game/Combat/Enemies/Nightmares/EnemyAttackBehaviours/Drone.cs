using Game.Combat.Misc;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Drone : CanTakeDamage
    {
        private float _angleOffset, _orbitRadius;
        private Rigidbody2D _rigidBody;
        private Transform _target;
        private static GameObject _prefab;

        protected override void Awake()
        {
            base.Awake();
            _rigidBody = GetComponent<Rigidbody2D>();
            ArmourController.AutoGenerateArmour();
            HealthController.SetInitialHealth(WorldState.ScaleValue(200), this);
            Heavyshot shot = gameObject.AddComponent<Heavyshot>();
            float minTime = Random.Range(3f, 6f);
            float maxTime = Random.Range(minTime + 1f, minTime + 2f);
            shot.Initialise(maxTime, minTime, 3, 0.2f);
        }

        public static Drone Create(Transform targetTransform, float radius, float angleOffset)
        {
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Drone");
            GameObject droneObject = Instantiate(_prefab);
            Drone drone = droneObject.GetComponent<Drone>();
            Vector2 startPosition = AdvancedMaths.CalculatePointOnCircle(angleOffset, radius, targetTransform.position);
            TeleportInOnly.TeleportIn(startPosition);
            drone._angleOffset = angleOffset;
            drone._orbitRadius = radius;
            drone._target = targetTransform;
            return drone;
        }

        private void FixedUpdate() => _rigidBody.MovePosition(CalculateTargetPosition());

        private Vector2 CalculateTargetPosition() => AdvancedMaths.CalculatePointOnCircle((Time.timeSinceLevelLoad + _angleOffset) * 30, _orbitRadius, _target.position);

        public override string GetDisplayName() => "Drone";
    }
}