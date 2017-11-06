using Facilitating.UI.Elements;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
	private Image _cooldownRing;
	private EnhancedText _cooldownText;
	private readonly Color _cooldownNotReadyColor = new Color(1, 1, 1, 0.4f);
	
	// Use this for initialization
	private void Awake()
	{
		_cooldownRing = GetComponent<Image>();
		_cooldownText = Helper.FindChildWithName<EnhancedText>(gameObject, "Cooldown Text");
		_cooldownRing.fillAmount = 0;
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
		_cooldownRing.fillAmount = normalisedValue;
	}

	public void Reset()
	{
		UpdateCooldownFill(1);
	}
}
