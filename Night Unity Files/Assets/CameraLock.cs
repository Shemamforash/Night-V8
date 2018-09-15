using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class CameraLock : MonoBehaviour
{
    private static bool _lockedCamera;
    private static CameraLock _instance;
    private Transform _shaker, _player;
    private bool _inCombat;
    private EnhancedText _cameraLockText;

    private void Awake()
    {
        _inCombat = CombatManager.InCombat();
        _shaker = GameObject.Find("Shaker")?.transform;
        _player = GameObject.Find("Player")?.transform;
        _instance = this;
        _cameraLockText = gameObject.FindChildWithName<EnhancedText>("Text");
        GetComponent<Button>().onClick.AddListener(ToggleLockedCamera);
        UpdateCameraLock();
    }

    private void UpdateCameraLock()
    {
        if (!_inCombat) return;
        if (_lockedCamera)
        {
            _shaker.SetParent(_player);
            _shaker.localPosition = Vector2.zero;
            return;
        }

        _shaker.SetParent(null);
    }

    public void Update()
    {
        _cameraLockText.SetText(_lockedCamera ? "Camera Locked" : "Camera Free");
    }
    
    public void LateUpdate()
    {
        if (!_inCombat) return;
        if (!_lockedCamera) return;
        Vector3 playerPosition = PlayerCombat.Instance.transform.position;
        playerPosition.z = transform.position.z;
        transform.position = playerPosition;
    }

    private static void ToggleLockedCamera()
    {
        _lockedCamera = !_lockedCamera;
        SaveController.SaveSettings();
        if (_instance == null || _instance.gameObject == null) return;
        _instance.UpdateCameraLock();
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Locked", _lockedCamera);
    }

    public static void Load(XmlNode root)
    {
        _lockedCamera = root.BoolFromNode("Locked");
    }
}