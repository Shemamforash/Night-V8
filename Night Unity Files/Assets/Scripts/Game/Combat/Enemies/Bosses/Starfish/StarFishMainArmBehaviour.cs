using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

public class StarFishMainArmBehaviour : MonoBehaviour
{
	private bool                 _armActive = true;
	private AudioSource          _audio;
	private bool                 _exploding;
	private StarFishArmBehaviour _firstArmSegment;

	private void Awake()
	{
		float angleOffset = transform.rotation.eulerAngles.z + 90f;
		_firstArmSegment = gameObject.FindChildWithName<StarFishArmBehaviour>("Arm 1");
		_firstArmSegment.SetOffset(angleOffset, transform, transform, 2, 1);
		_audio = _firstArmSegment.GetComponent<AudioSource>();
	}

	public void UpdateAngle(float zAngle)
	{
		if (StarfishBehaviour.ShouldPlayAudio()) _audio.Play();
		if (!_armActive) return;
		if (AllArmsDead(_firstArmSegment))
		{
			_armActive = false;
			StartCoroutine(ExplodeArms());
		}

		if (_firstArmSegment == null) return;
		_firstArmSegment.SetPosition(zAngle);
	}

	private bool AllArmsDead(StarFishArmBehaviour arm)
	{
		if (arm == null) return true;
		return arm.Dead() && AllArmsDead(arm.NextArm());
	}

	private IEnumerator ExplodeArms()
	{
		List<StarFishArmBehaviour> arms = new List<StarFishArmBehaviour>();
		StarFishArmBehaviour       arm  = _firstArmSegment;
		while (arm != null)
		{
			arms.Add(arm);
			arm = arm.NextArm();
		}

		arms.Reverse();
		foreach (StarFishArmBehaviour a in arms)
		{
			LeafBehaviour.CreateLeaves(a.transform.position);
			Destroy(a.gameObject);
			yield return new WaitForSeconds(0.1f);
		}
	}
}