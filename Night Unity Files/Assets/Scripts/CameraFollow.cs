using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private Transform _playerTransform;
	
	private void Start()
	{
		if (PlayerCombat.Instance == null) return;
		_playerTransform = PlayerCombat.Instance.transform;
	}


	public void LateUpdate()
	{
		if (!CombatManager.IsCombatActive()) return;
		if (CameraLock.IsCameraLocked()) transform.rotation = _playerTransform.rotation;
		Vector3 playerPosition = _playerTransform.position;
		playerPosition.z = transform.position.z;
		transform.position = playerPosition;
	}
}
