using System.Collections.Generic;
using SamsHelper;
using UnityEngine;

public class FootstepMaker : MonoBehaviour {
	private const float DistanceToFootPrint = 1f;
	private float _distanceTravelled;
	private bool _leftLast;
	private Vector3 _lastPosition;
	private GameObject _footprintPrefab;
	private readonly List<GameObject> _footstepPool = new List<GameObject>();
	private Transform _footstepParent;
	private Rigidbody2D _rigidBody;

	public void Awake()
	{
		if(_footstepParent == null) _footstepParent = GameObject.Find("World").transform.Find("Footsteps");
		_rigidBody = GetComponent<Rigidbody2D>();
	}

	private void SetTransformAndRotation(GameObject footprintObject)
	{
		footprintObject.SetActive(true);
		footprintObject.transform.position = transform.position;
		footprintObject.transform.rotation = GetRotation();
	}
	
	private GameObject GetNewFootprint()
	{
		GameObject footprintObject;
		if (_footstepPool.Count == 0)
		{
			if(_footprintPrefab == null) _footprintPrefab = Resources.Load<GameObject>("Prefabs/Map/Footprint");
			footprintObject = Instantiate(_footprintPrefab, transform.position, GetRotation());
			footprintObject.transform.SetParent(_footstepParent);
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

	private Quaternion GetRotation()
	{
		float zRotation = AdvancedMaths.AngleFromUp(Vector3.zero, _rigidBody.velocity.normalized);
		return Quaternion.Euler(new Vector3(0, 0, zRotation));
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
