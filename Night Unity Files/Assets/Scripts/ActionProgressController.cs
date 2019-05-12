using DG.Tweening;
using Extensions;
using Game.Characters.CharacterActions;
using TMPro;
using UnityEngine;

public class ActionProgressController : MonoBehaviour
{
	private CanvasGroup         _barCanvas;
	private float               _lastProgress = -1;
	private BaseCharacterAction _lastState;
	private SteppedProgressBar  _left,       _right;
	private Tweener             _rightTween, _leftTween;
	private TextMeshProUGUI     _text;


	private void Awake()
	{
		GameObject actionProgressObject = gameObject.FindChildWithName("Action Progress");
		_barCanvas = gameObject.FindChildWithName<CanvasGroup>("Bar");
		_left      = actionProgressObject.FindChildWithName<SteppedProgressBar>("Left");
		_right     = actionProgressObject.FindChildWithName<SteppedProgressBar>("Right");
		_text      = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
	}

	public void UpdateCurrentAction(BaseCharacterAction state)
	{
		if (state == null) return;
		_text.text = state.GetDisplayName();
		float progress             = state.GetNormalisedProgress();
		if (progress < 0) progress = 0;
		if (progress == _lastProgress) return;
		_lastProgress = progress;
		_left.SetValue(progress);
		_right.SetValue(progress);
		if (_lastState == state && !state.ForceViewUpdate) return;
		state.ForceViewUpdate = false;
		_barCanvas.DOFade(state is Rest ? 0f : 1f, 0.5f);
		_lastState = state;
	}
}