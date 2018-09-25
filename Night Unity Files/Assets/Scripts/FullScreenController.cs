using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenController : MonoBehaviour {
	private static bool _fullScreen;
	private EnhancedText _fullscreenText;

	private void Awake()
	{
		_fullscreenText = gameObject.FindChildWithName<EnhancedText>("Text");
		GetComponent<Button>().onClick.AddListener(SwitchFullscreen);
	}

	public void Update()
	{
		_fullscreenText.SetText(_fullScreen ? "Fullscreen" : "Windowed");
	}

	public void SwitchFullscreen()
	{
		_fullScreen = !_fullScreen;
		if (_fullScreen)
		{
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
			return;
		}

		Screen.SetResolution((int) (Screen.currentResolution.width * 0.75f), (int) (Screen.currentResolution.height * 0.75f), false);
		SaveController.SaveSettings();
	}

	public static void Load(XmlNode root)
	{
		_fullScreen = root.BoolFromNode("Fullscreen");
		if (!_fullScreen)
		{
			int width = root.IntFromNode("Width");
			int height = root.IntFromNode("Height");
			Screen.SetResolution(width, height, false);
		}
		else
		{
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
		}
	}

	public static void Save(XmlNode root)
	{
		root.CreateChild("Fullscreen", _fullScreen);
		root.CreateChild("Width", Camera.main.pixelWidth);
		root.CreateChild("Height", Camera.main.pixelHeight);
	}
}
