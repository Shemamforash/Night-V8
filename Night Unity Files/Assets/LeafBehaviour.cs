using UnityEngine;

public class LeafBehaviour : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == 8) return;
		Vector3 velocity = other.transform.parent.GetComponent<Rigidbody2D>().velocity;
		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		Vector3 dir = other.transform.position - transform.position;
		dir.Normalize();
		dir += velocity.normalized;
		dir.Normalize();
		rigidbody2D.AddForce(dir * velocity.magnitude);
	}
}
