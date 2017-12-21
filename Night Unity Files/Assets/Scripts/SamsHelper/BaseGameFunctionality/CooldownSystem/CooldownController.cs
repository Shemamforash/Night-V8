using Facilitating.UI.Elements;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
	private Slider _cooldownSlider;
	private EnhancedText _cooldownText;
	private readonly Color _cooldownNotReadyColor = new Color(1, 1, 1, 0.4f);
	
	// Use this for initialization
	private void Awake()
	{
		_cooldownSlider = Helper.FindChildWithName<Slider>(gameObject, "Cooldown Bar");
		_cooldownText = Helper.FindChildWithName<EnhancedText>(gameObject, "Cooldown Text");
		_cooldownSlider.value = 0;
		UpdateCooldownFill(1);
	}

	public void Text(string text)
	{
		_cooldownText.Text(text);
	}

	public void UpdateCooldownFill(float normalisedValue)
	{
		Color targetColor = _cooldownNotReadyColor;
		if (normalisedValue == 1)
		{
			targetColor = Color.white;
		}
		_cooldownText.SetColor(targetColor);
		_cooldownSlider.value = normalisedValue;
	}

	public void Reset()
	{
		UpdateCooldownFill(1);
	}
}
