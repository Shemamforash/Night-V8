using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour
{
	private CanvasGroup _menuCanvasGroup;
	private static bool _shownSplashScreen;
	private Image _logo, _fireBackground;
	private ParticleSystem _fireParticles;
	
	public void Awake ()
	{
		_menuCanvasGroup = gameObject.FindChildWithName<CanvasGroup>("Menu Canvas Group");
		_logo = gameObject.FindChildWithName<Image>("Logo");
		_fireBackground = gameObject.FindChildWithName<Image>("Fire Image");
		_fireParticles = GameObject.Find("Fire").GetComponent<ParticleSystem>();
		_menuCanvasGroup.alpha = 0f;
		_logo.color = UiAppearanceController.InvisibleColour;
		_fireBackground.color = UiAppearanceController.InvisibleColour;
		Sequence s = DOTween.Sequence();
		s.AppendInterval(1f);
		s.Append(_logo.DOColor(Color.white, 1f));
		s.AppendInterval(2f);
		s.Append(_logo.DOColor(UiAppearanceController.InvisibleColour, 1f));
		s.AppendCallback(() => _fireParticles.Play());
		s.Append(_fireBackground.DOColor(new Color(1f, 0.4f, 0f, 1f), 1));
		s.Join(_menuCanvasGroup.DOFade(1, 3f));
		GameObject.Find("New Game").GetComponent<Button>().Select();
	}
}
