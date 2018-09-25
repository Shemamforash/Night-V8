using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

public class ActionProgressController : MonoBehaviour
{
	private SteppedProgressBar _left, _right;
	private TextMeshProUGUI _text;
	private float _lastRemainingTime;

	private void Awake()
	{
		GameObject actionProgressObject = gameObject.FindChildWithName("Action Progress");
		_left = actionProgressObject.FindChildWithName<SteppedProgressBar>("Left");
		_right = actionProgressObject.FindChildWithName<SteppedProgressBar>("Right");
		_text = GetComponent<TextMeshProUGUI>();
	}

	public void UpdateCurrentAction(BaseCharacterAction state)
	{
		if (state is Rest)
		{
			gameObject.SetActive(false);
			return;
		}
		UpdateCurrentActionText(state);
		gameObject.SetActive(true);
		string actionString = state.GetDisplayName();
		_text.text = actionString;
		_lastRemainingTime = state.GetRemainingTime();
	}

	private void UpdateCurrentActionText(BaseCharacterAction state)
	{
		float currentTime = state.GetRemainingTime();
		if (currentTime == _lastRemainingTime) return;
		_lastRemainingTime = currentTime;
		_left.SetValue(_lastRemainingTime);
		_right.SetValue(_lastRemainingTime);
	}
}
