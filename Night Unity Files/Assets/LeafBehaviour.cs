using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
	private Rigidbody2D _rb2d;
	
	private void Awake()
	{
		_rb2d = GetComponent<Rigidbody2D>();
	}
	
	public static void CreateLeaves()
	{
		List<GameObject> prefabs = new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/Combat/Leaves"));
		for (int i = 0; i < Random.Range(5, 10); ++i)
		{
			Vector2 origin = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 17f);
			for (int j = 0; j < Random.Range(15, 30); ++j)
			{
				Vector2 position = AdvancedMaths.RandomVectorWithinRange(Vector2.one, 0.5f);
				position.x = Mathf.Pow(position.x, 2);
				position.y = Mathf.Pow(position.y, 2);
				position += origin;
				GameObject leafObject = Instantiate(prefabs.RandomElement());
				leafObject.transform.position = position;
				leafObject.transform.localScale = Random.Range(0.1f, 0.2f) * Vector2.one;
				leafObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
			}
		}

		for (int j = 0; j < Random.Range(15, 30); ++j)
		{
			Vector2 position = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 17f);
			GameObject leafObject = Instantiate(prefabs.RandomElement());
			leafObject.transform.position = position;
			leafObject.transform.localScale = Random.Range(0.1f, 0.2f) * Vector2.one;
			leafObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
		}
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == 8) return;
		Vector3 velocity = other.transform.parent.GetComponent<Rigidbody2D>().velocity;
		float distance = other.transform.Distance(transform);
		distance /= 0.5f;
		_rb2d.AddForce(velocity * 2 * distance * Random.Range(0.8f, 1.1f));
	}
}
