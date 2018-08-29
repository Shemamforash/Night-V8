using Facilitating.Audio;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour, IInputListener
{
	private bool _open;
	private string _lastMenu;
	private static bool _fullScreen;
	[SerializeField] private EnhancedText _title;
	[SerializeField] private Slider _volumeSlider;
	[SerializeField] private EnhancedText _fullscreenText;
	
	public void Awake()
	{
		InputHandler.RegisterInputListener(this);
		_volumeSlider.onValueChanged.AddListener(GlobalAudioManager.SetMasterVolume);
		_volumeSlider.value = GlobalAudioManager.Volume();
	}

	public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
	{
		if (isHeld || axis != InputAxis.Menu) return;
		if (_open) Hide();
		else Show();
	}

	private void Show()
	{
		_lastMenu = MenuStateMachine.CurrentMenu().gameObject.name;
		ShowPauseMenu();
		_open = true;
		_title.gameObject.SetActive(true);
	}

	public void Hide()
	{
		MenuStateMachine.ShowMenu(_lastMenu);
		_open = false;
		_title.gameObject.SetActive(false);
	}

	public void OnInputUp(InputAxis axis)
	{
	}

	public void OnDoubleTap(InputAxis axis, float direction)
	{
	}

	public void ShowOptions()
	{
		MenuStateMachine.ShowMenu("Options Menu");
		_volumeSlider.value = GlobalAudioManager.Volume();
		_title.SetText("Options");
		SetFullScreenText();
	}

	private void SetFullScreenText()
	{
		_fullscreenText.SetText(_fullScreen ? "Fullscreen" : "Windowed");
	}

	public void ShowControls()
	{
		MenuStateMachine.ShowMenu("Controls Menu");
		_title.SetText("Controls");
	}

	public void ShowPauseMenu()
	{
		MenuStateMachine.ShowMenu("Pause Sub Menu");
		_title.SetText("Paused");
	}

	public void Exit()
	{
		Application.Quit();
	}

	public void SwitchFullscreen()
	{
		_fullScreen = !_fullScreen;
		SetFullScreenText();
		if (_fullScreen)
		{
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
			return;
		} 
		Screen.SetResolution((int) (Screen.currentResolution.width * 0.75f), (int) (Screen.currentResolution.height * 0.75f), false);
	}
}
