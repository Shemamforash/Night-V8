using DG.Tweening;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UIBorderController : MonoBehaviour
{
	private const            float          FadeTime = 0.25f;
	private                  EnhancedButton _button;
	private                  CanvasGroup    _canvasGroup;
	[SerializeField] private BorderState    _currentState;
	private                  bool           _needsUpdate;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		SetDisabled();
	}

	private void SetState(BorderState state)
	{
		_currentState = state;
		if (!gameObject.activeInHierarchy)
		{
			_needsUpdate = true;
		}
		else
		{
			UpdateBorder();
		}
	}

	private void OnEnable()
	{
		if (!_needsUpdate) return;
		UpdateBorder();
		_needsUpdate = false;
	}

	private void OnDisable()
	{
		_currentState = BorderState.Disabled;
		UpdateBorder(true);
	}

	private void UpdateBorder(bool instant = false)
	{
		bool currentTimeScale = DOTween.defaultTimeScaleIndependent;
		DOTween.defaultTimeScaleIndependent = true;
		switch (_currentState)
		{
			case BorderState.Disabled:
				Disable(instant);
				break;
			case BorderState.Active:
				Active(instant);
				break;
			case BorderState.Selected:
				Select(instant);
				break;
		}

		DOTween.defaultTimeScaleIndependent = currentTimeScale;
	}

	private void Disable(bool instant = false)
	{
		float time = instant ? 0 : FadeTime;
		_canvasGroup.DOFade(0, time);
	}

	private void Active(bool instant = false)
	{
		float time = instant ? 0 : FadeTime;
		_canvasGroup.DOFade(0.4f, time);
	}

	private void Select(bool instant = false)
	{
		float time = instant ? 0 : FadeTime;
		_canvasGroup.DOFade(1f, time);
	}

	public void SetDisabled()
	{
		SetState(BorderState.Disabled);
	}

	public void SetActive()
	{
		SetState(BorderState.Active);
	}

	public void SetSelected()
	{
		SetState(BorderState.Selected);
	}

	public void SetButton(EnhancedButton enhancedButton)
	{
		_button = enhancedButton;
		transform.SetParent(_button.transform, false);
	}

	private enum BorderState
	{
		Disabled,
		Active,
		Selected
	}
}