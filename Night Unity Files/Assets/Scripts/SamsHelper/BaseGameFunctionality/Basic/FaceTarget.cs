using SamsHelper.Libraries;
using UnityEngine;

public class FaceTarget : MonoBehaviour
{
	[SerializeField] private Transform _target;

	private void Awake()
	{
		if (_target != null) return;
		int parentIndex = transform.GetSiblingIndex() - 1;
		if (parentIndex == -1)
		{
			_target = transform.parent;
			return;
		}

		_target = transform.parent.GetChild(parentIndex);
	}

	private void FixedUpdate()
	{
		if (_target == null) return;
		float rot = AdvancedMaths.AngleFromUp(transform.position, _target.position);
		transform.rotation = Quaternion.Euler(0, 0, rot);
	}
}