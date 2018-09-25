using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class CameraLock : MonoBehaviour
{
    private static bool _lockedCamera = true;
    private EnhancedText _cameraLockText;

    private void Awake()
    {
        _cameraLockText = gameObject.FindChildWithName<EnhancedText>("Text");
        GetComponent<Button>().onClick.AddListener(ToggleLockedCamera);
    }

    public void Update()
    {
        _cameraLockText.SetText(_lockedCamera ? "Camera Locked" : "Camera Free");
    }

    public static bool IsCameraLocked()
    {
        return _lockedCamera;
    }

    private static void ToggleLockedCamera()
    {
        _lockedCamera = !_lockedCamera;
        SaveController.SaveSettings();
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