using SamsHelper.Libraries;
using UnityEngine;

public class FaceVelocity : MonoBehaviour {
	private Rigidbody2D _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		float rot = AdvancedMaths.AngleFromUp(Vector2.zero, _rigidbody.velocity);
		transform.rotation = Quaternion.Euler(0, 0, rot);
	}
}
