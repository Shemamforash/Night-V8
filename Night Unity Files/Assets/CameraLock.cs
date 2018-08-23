using Game.Combat.Player;
using UnityEngine;

public class CameraLock : MonoBehaviour
{
    private bool _lockedCamera = false;

    private void Awake()
    {
        if (!_lockedCamera) return;
        Transform shaker = transform.parent;
        shaker.SetParent(null);
    }

    public void LateUpdate()
    {
        if (!_lockedCamera) return;
        Vector3 playerPosition = PlayerCombat.Instance.transform.position;
        playerPosition.z = transform.position.z;
        transform.position = playerPosition;
    }
}