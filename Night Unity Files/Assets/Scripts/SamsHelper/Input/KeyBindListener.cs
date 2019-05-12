
using Extensions;
using SamsHelper.Input;
using TMPro;
using UnityEngine;

public class KeyBindListener : ControlTypeChangeListener
{
	[SerializeField] private InputAxis       _targetAxis;
	private                  TextMeshProUGUI _text;

	private void Awake()
	{
		_text = gameObject.FindChildWithName<TextMeshProUGUI>("Key");
	}

	private void Start()
	{
		SetOnControllerInputChange(UpdateText);
	}

	private void UpdateText()
	{
		string keyText = InputHandler.GetBindingForKey(_targetAxis);
		switch (_targetAxis)
		{
			case InputAxis.Horizontal:
				keyText += InputHandler.GetBindingForKey(InputAxis.Vertical);
				keyText =  keyText == "A - DW - S" ? "WASD" : "Left Stick";
				break;
			case InputAxis.SkillOne:
				keyText = keyText == "Num 1" ? "Num 1-4" : "DPAD";
				break;
			case InputAxis.SwitchTab:
				if (keyText != "J - L") keyText = "Right Stick";
				break;
		}

		_text.text = keyText;
	}
}