using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmBehaviour : Boss
{
	private static SwarmBehaviour _instance;
	private const float MoveSpeed = 10;
	public int SwarmCount = 100;
	public float _burstTimer = 2f;

	public override void Awake()
	{
		base.Awake();
		_instance = this;
		GameObject swarmPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Segment");
		for (int i = 0; i < SwarmCount; ++i)
		{
			Instantiate(swarmPrefab).transform.SetParent(transform);
		}
		Sections.ForEach(s => ((SwarmSegmentBehaviour)s).SetNeighbors(Sections));
	}
	
	public void FixedUpdate()
	{
		Vector2 mousePosition = Helper.MouseToWorldCoordinates();
		Vector2 dir = (mousePosition - (Vector2)transform.position).normalized;
//		RigidBody.AddForce(dir * MoveSpeed);
	}

	public void Update()
	{
		float burstForce = 0f;
		_burstTimer -= Time.deltaTime;
		if (_burstTimer < 0f)
		{
			_burstTimer = Random.Range(1f, 2f);
			burstForce = Random.Range(10f, 20f);
		}
		Sections.ForEach(s => ((SwarmSegmentBehaviour)s).UpdateSection(transform.position, burstForce));
	}

	public static SwarmBehaviour Instance()
	{
		return _instance;
	}
}
