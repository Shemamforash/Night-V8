using UnityEngine;

namespace Game.Combat.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementController : MonoBehaviour
    {
        private const float DashForce = 300;
        private Rigidbody2D _rigidbody;
        private Vector2 _forceToadd = Vector2.zero;
        public float _speed;

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
        
        public void FixedUpdate()
        {
            _rigidbody.AddForce(_forceToadd);
            _forceToadd = Vector2.zero;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public void Dash(Vector2 direction)
        {
            _forceToadd += direction * DashForce;
        }

        public void Move(Vector2 direction)
        {
            float speed = _speed;
            _forceToadd += direction * speed * Time.deltaTime / 0.016f;
        }

        public void Knockback(Vector3 source, float force = 10f)
        {
            Vector3 direction = (transform.position - source).normalized;
            _forceToadd = direction * force;
        }

        public void Ram(Transform target, float ramForce)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            _forceToadd += direction * ramForce;
        }

        public void AddForce(Vector2 force)
        {
            _forceToadd += force;
        }

        public bool Moving() => _rigidbody.velocity == Vector2.zero;
    }
}