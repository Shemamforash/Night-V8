﻿using Game.Combat.Player;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _playerTransform;
    private Vector3 _position;
    private Quaternion _rotation;

    private void Start()
    {
        if (PlayerCombat.Instance == null) return;
        _playerTransform = PlayerCombat.Instance.transform;
    }

    public void LateUpdate()
    {
        if (PlayerCombat.Instance == null)
        {
            transform.position = _position;
            transform.rotation = _rotation;
            return;
        }

        if (PlayerCombat.Instance.IsKeyboardBeingUsed()) _rotation = _playerTransform.rotation;
        transform.rotation = _rotation;
        _position = _playerTransform.position;
        _position.z = transform.position.z;
        transform.position = _position;
    }
}