using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

public class MarkController : MonoBehaviour
{
	private static readonly List<MarkController> _marks = new List<MarkController>();
	private static          GameObject           _prefab;
	private                 float                _lifeTime = 10f;

	public static void Create(Vector2 position)
	{
		if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Mark");
		GameObject markObject        = Instantiate(_prefab);
		markObject.transform.position = position;
		_marks.Add(markObject.GetComponent<MarkController>());
	}

	public void Update()
	{
		_lifeTime -= Time.deltaTime;
		if (_lifeTime > 0f) return;
		Destroy(this);
	}

	public static bool InMarkArea(Vector2 position)
	{
		foreach (var m in _marks)
		{
			if (position.Distance(m.transform.position) < 3)
			{
				return true;
			}
		}

		return false;
	}

	private void OnDestroy()
	{
		_marks.Remove(this);
	}
}