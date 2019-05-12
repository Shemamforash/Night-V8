using DG.Tweening;
using Game.Characters;
using Game.Global;
using Extensions;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class CombatStoryController : MonoBehaviour
{
	private static JournalEntry          _journalHere;
	public static  bool                  ShouldShow;
	private static Player                _player;
	private        CloseButtonController _closeButton;
	private        CanvasGroup           _storyCanvas;
	private        EnhancedText          _titleText, _contentText;

	public void Awake()
	{
		_storyCanvas = gameObject.FindChildWithName<CanvasGroup>("Foreground");
		_closeButton = _storyCanvas.gameObject.FindChildWithName<CloseButtonController>("Close Button");
		_closeButton.SetOnClick(FadeOut);
		_titleText   = _storyCanvas.gameObject.FindChildWithName<EnhancedText>("Title");
		_contentText = _storyCanvas.gameObject.FindChildWithName<EnhancedText>("Content");
		_titleText.SetText(_journalHere.Title);
		_contentText.SetText(_journalHere.Text);
		_journalHere.Unlock();
		Sequence sequence = DOTween.Sequence();
		sequence.Append(_storyCanvas.DOFade(1f, 1f));
		sequence.AppendCallback(_closeButton.Enable);
	}

	public void Start()
	{
		_closeButton.UseSpaceInput();
	}

	private void FadeOut()
	{
		_closeButton.Disable();
		_storyCanvas.DOFade(0f, 5f);
		ScreenFaderController.FadeIn(5f);
		SceneChanger.GoToCombatScene(_player);
	}

	public static void TryEnter(Player player)
	{
		if (ShouldShow)
		{
			_player = player;
			SceneChanger.GoToCombatStoryScene();
			_journalHere = JournalEntry.GetStoryEntry();
			ShouldShow   = false;
		}
		else
		{
			SceneChanger.GoToCombatScene(player);
		}
	}
}