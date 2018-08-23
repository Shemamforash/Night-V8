using SamsHelper.Libraries;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
	[SerializeField] private float Force;
	private Rigidbody2D _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}
	
	private void FixedUpdate ()
	{
		Vector3 mousePos = Helper.MouseToWorldCoordinates();
		Vector3 dir = mousePos - transform.position;
		dir.Normalize();
		_rigidbody.AddForce(dir * Force);
	}
}
