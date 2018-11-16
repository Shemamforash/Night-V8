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
    private Image _cursorImage;

    private void Awake()
    {
        _cameraLockText = gameObject.FindChildWithName<EnhancedText>("Text");
        GetComponent<Button>().onClick.AddListener(ToggleLockedCamera);
        SetLock(_lockedCamera);
    }

    public void Update()
    {
        string inputString = "Input Mode:\n";
        inputString += _lockedCamera ? "Keyboard Only" : "Mouse and Keyboard";
        _cameraLockText.SetText(inputString);
    }

    public static bool IsCameraLocked()
    {
        return _lockedCamera;
    }

    private static void ToggleLockedCamera()
    {
        SetLock(!_lockedCamera);
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Locked", _lockedCamera);
    }

    private static void SetLock(bool locked)
    {
        _lockedCamera = locked;
        SaveController.SaveSettings();
    }

    public static void Load(XmlNode root)
    {
        SetLock(root.BoolFromNode("Locked"));
    }
}