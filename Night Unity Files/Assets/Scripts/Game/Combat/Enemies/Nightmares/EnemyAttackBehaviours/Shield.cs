using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Shield : TimedAttackBehaviour
    {
        private static GameObject _shieldPrefab;
        private GameObject _shieldObject;
        
        public override void Awake()
        {
            base.Awake();
            if (_shieldPrefab == null) _shieldPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Shield");
            _shieldObject = Instantiate(_shieldPrefab);
            _shieldObject.transform.SetParent(transform, false);
            _shieldObject.transform.localScale = Vector3.one;
        }
        
        protected override void Attack()
        {
            Destroy(_shieldObject);
            Destroy(this);
        }
    }
}