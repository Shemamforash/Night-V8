using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

public class StarFishArmBehaviour : BossSectionHealthController {
	private StarFishArmBehaviour NextArm;
	private readonly Queue<Vector3> _parentPositions = new Queue<Vector3>();
	private readonly Queue<float> _parentRotations = new Queue<float>();
	private float _radius;
	private float _offset;
	private const float DampingModifier = 0.98f;
	private float Damping;
	private Transform _prevSegment;

	public override void Start()
	{
		SetBoss(StarfishBehaviour.Instance());
		base.Start();
	}
	
	protected override int GetInitialHealth()
	{
		return 50;
	}

	public void SetPosition(float angle)
	{
		float newAngle = _offset + angle * Damping;
		Vector2 pos = AdvancedMaths.CalculatePointOnCircle(newAngle, _radius * StarfishBehaviour.GetRadiusModifier(), Vector2.zero);
		_parentPositions.Enqueue(pos);
		_parentRotations.Enqueue(angle);

		if (_parentPositions.Count < 10) return;

		Vector3 newPosition = _parentPositions.Dequeue();
		transform.position = newPosition;
		float rot = AdvancedMaths.AngleFromUp(_prevSegment.position, transform.position);
		transform.rotation = Quaternion.Euler(0, 0, rot);

		SetNextSegmentPosition(_parentRotations.Dequeue());
	}

	public void SetOffset(float offset, Transform root, Transform prevSegment, int i, float damping)
	{
		_radius = 0.65f + i * 0.25f;
		_offset = offset;
		Damping = damping;
		_prevSegment = prevSegment;
		transform.position = root.transform.position;
		Transform wingObject = root.Find("Arm " + i);
		if (wingObject == null) return;
		NextArm = wingObject.GetComponent<StarFishArmBehaviour>();
		if (NextArm == null) return;
		NextArm.SetOffset(offset, root, transform, i + 1, damping * DampingModifier);
	}

	private void SetNextSegmentPosition(float angle)
	{
		if (NextArm == null) return;
		NextArm.SetPosition(angle);
	}

	public override void Kill()
	{
		base.Kill();
		Detach(false);
	}

	public void OnDestroy()
	{
		StarfishBehaviour.UnregisterArm(this);
	}

	private void Detach(bool spawn = true)
	{
		StarFishArmBehaviour _previousArm = _prevSegment.GetComponent<StarFishArmBehaviour>();
		if (_previousArm != null) _previousArm.NextArm = null;
		Destroy(gameObject);

		if (NextArm != null)
		{
			NextArm.Detach();
		}

		if (spawn)
		{
			CombatManager.SpawnEnemy(EnemyType.Ghoul, transform.position);
		}
	}
}
