using System.Collections.Generic;
using SamsHelper;
using UnityEngine;

public class FootstepMaker : MonoBehaviour {
	private const float DistanceToFootPrint = 1f;
	private float _distanceTravelled;
	private bool _leftLast;
	private Vector3 _lastPosition;
	private GameObject _footprintPrefab;
	private static readonly List<GameObject> _footstepPool = new List<GameObject>();
	private Quaternion _rotation;

	public void Awake()
	{
		_footprintPrefab = Resources.Load<GameObject>("Prefabs/Map/Footprint");
	}

	private void SetTransformAndRotation(GameObject footprintObject)
	{
		footprintObject.SetActive(true);
		footprintObject.transform.position = transform.position;
		footprintObject.transform.rotation = _rotation;
	}
	
	private GameObject GetNewFootprint()
	{
		GameObject footprintObject;
		if (_footstepPool.Count == 0)
		{
			footprintObject = Instantiate(_footprintPrefab, transform.position, _rotation);
			FadeAndDie footprint = footprintObject.GetComponent<FadeAndDie>();
			footprint.SetLifeTime(2f, () =>
			{
				_footstepPool.Add(footprint.gameObject);
				footprint.gameObject.SetActive(false);
			});
		}
		else
		{
			footprintObject = _footstepPool[0];
			_footstepPool.RemoveAt(0);
			SetTransformAndRotation(footprintObject);
		}
		footprintObject.GetComponent<FadeAndDie>().StartFade();
		return footprintObject;
	}

	public void UpdateRotation(float rotation)
	{
		_rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
	}
	
	private void Update()
	{
		_distanceTravelled += Vector3.Distance(_lastPosition, transform.position);
		if (_distanceTravelled < DistanceToFootPrint) return;
		if (_leftLast)
		{
			GetNewFootprint().transform.Translate(Vector3.left * 0.03f);
			_leftLast = false;
		}
		else
		{
			GetNewFootprint().transform.Translate(Vector3.right * 0.03f);
			_leftLast = true;
		}

		_distanceTravelled = 0;
		_lastPosition = transform.position;
	}
	
}
