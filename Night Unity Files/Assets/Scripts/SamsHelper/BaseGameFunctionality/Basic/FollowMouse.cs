
using Extensions;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
	private                  Rigidbody2D _rigidbody;
	[SerializeField] private float       Force;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		Vector3 mousePos = Helper.MouseToWorldCoordinates();
		Vector3 dir      = mousePos - transform.position;
		dir.Normalize();
		_rigidbody.AddForce(dir * Force);
	}
}