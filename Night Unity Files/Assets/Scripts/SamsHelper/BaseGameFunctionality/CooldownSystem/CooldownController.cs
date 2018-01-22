using Facilitating.UI.Elements;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
	private Image _cooldownFill;
	private EnhancedText _cooldownText;
	private readonly Color _cooldownNotReadyColor = new Color(1, 1, 1, 0.4f);
	
	// Use this for initialization
	private void Awake()
	{
		_cooldownFill = Helper.FindChildWithName<Image>(gameObject, "Fill");
		_cooldownText = Helper.FindChildWithName<EnhancedText>(gameObject, "Text");
		_cooldownFill.fillAmount = 1;
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
		_cooldownFill.fillAmount = normalisedValue;
	}

	public void Reset()
	{
		UpdateCooldownFill(1);
	}
}
