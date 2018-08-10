using SamsHelper.Libraries;
using UnityEngine;

public class StarFishMainArmBehaviour : MonoBehaviour
{
	private float _currentAngle;
	private StarFishArmBehaviour _firstArmSegment;

	private void Awake()
	{
		float angleOffset = transform.rotation.eulerAngles.z + 90f;
		_firstArmSegment = gameObject.FindChildWithName<StarFishArmBehaviour>("Arm 1");
		_firstArmSegment.SetOffset(angleOffset, transform, transform, 2, 1);
	}

	public void UpdateAngle(float zAngle)
	{
		if (_firstArmSegment == null) return;
		_firstArmSegment.SetPosition(zAngle);
	}
}
