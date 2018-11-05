using UnityEngine;

namespace Game.Combat.Ui
{
    public class FirePath : MonoBehaviour
    {
        private const float FireDropDistance = 0.2f;
        private Vector3 _direction = Vector3.zero;
        private Vector3 _lastDropPosition;

        public void Awake()
        {
            _lastDropPosition = transform.position;
        }

        public void Update()
        {
            float distanceTravelled = Vector2.Distance(_lastDropPosition, transform.position);
            _direction = (transform.position - _lastDropPosition).normalized;
            for (float i = FireDropDistance; i < distanceTravelled; i += FireDropDistance)
            {
                Vector2 dropPosition = _lastDropPosition + _direction * FireDropDistance;
                _lastDropPosition = dropPosition;
                TrailFireBehaviour.Create(dropPosition);
            }
        }
    }
}