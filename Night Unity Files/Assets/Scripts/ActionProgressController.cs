using DG.Tweening;
using Game.Characters.CharacterActions;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

public class ActionProgressController : MonoBehaviour
{
    private SteppedProgressBar _left, _right;
    private CanvasGroup _barCanvas;
    private TextMeshProUGUI _text;
    private float _lastRemainingTime;
    private BaseCharacterAction _lastState;

    private void Awake()
    {
        GameObject actionProgressObject = gameObject.FindChildWithName("Action Progress");
        _barCanvas = gameObject.FindChildWithName<CanvasGroup>("Bar");
        _left = actionProgressObject.FindChildWithName<SteppedProgressBar>("Left");
        _right = actionProgressObject.FindChildWithName<SteppedProgressBar>("Right");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public void UpdateCurrentAction(BaseCharacterAction state)
    {
        bool newStateIsRest = state is Rest;
        bool oldStateIsRest = _lastState is Rest;
        if (newStateIsRest && !oldStateIsRest) _barCanvas.DOFade(0f, 0.5f);
        else if (!newStateIsRest && oldStateIsRest) _barCanvas.DOFade(1f, 0.5f);
        if (state == null) return;
        UpdateCurrentActionTime(state);
        _text.text = state.GetDisplayName();
        _lastRemainingTime = state.GetRemainingTime();
        _lastState = state;
    }

    private void UpdateCurrentActionTime(BaseCharacterAction state)
    {
        float currentTime = state.GetRemainingTime();
        if (currentTime == _lastRemainingTime) return;
        _left.SetValue(_lastRemainingTime);
        _right.SetValue(_lastRemainingTime);
    }
}