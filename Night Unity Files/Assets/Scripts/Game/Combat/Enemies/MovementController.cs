using UnityEngine;

namespace Game.Combat.Enemies
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class MovementController : MonoBehaviour
	{
		private const float       DashForce   = 300;
		private       Vector2     _forceToadd = Vector2.zero;
		private       Rigidbody2D _rigidbody;
		public        float       _speed;

		public void Awake()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
			MovementControllerManager.RegisterMovementController(this);
		}

		private void OnDestroy()
		{
			MovementControllerManager.UnregisterMovementController(this);
		}

		public void MyFixedUpdate()
		{
			_rigidbody.AddForce(_forceToadd);
			_forceToadd = Vector2.zero;
		}

		public void SetSpeed(float speed)
		{
			_speed = speed;
		}

		public void Dash()
		{
			_forceToadd += _rigidbody.velocity.normalized * DashForce;
		}

		public void Move(Vector2 direction)
		{
			float speed = _speed;
			_forceToadd += direction * speed * Time.deltaTime / 0.016f;
		}

		public void KnockBack(Vector3 direction, float force = 10f)
		{
			_rigidbody.AddForce(direction * force);
		}

		public void AddForce(Vector2 force)
		{
			_forceToadd += force;
		}

		public bool Moving() => _rigidbody.velocity == Vector2.zero;

		public Vector3 GetVelocity() => _rigidbody.velocity;
	}
}