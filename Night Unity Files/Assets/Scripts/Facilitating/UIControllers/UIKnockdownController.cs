using System.Collections;
using Game.Combat;
using SamsHelper;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKnockdownController : MonoBehaviour
{
    private static UIKnockdownController _instance;
    private static int _pressesRemaining, _startingPresses;
    private static int _lastPressDirection;
    private static TextMeshProUGUI _leftText, _rightText;
    private static Image _leftProgressBar, _rightProgressBar;

    public void Awake()
    {
        _leftText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Left Key");
        _rightText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Right Key");
        _leftProgressBar = Helper.FindChildWithName<Image>(gameObject, "Left Bar");
        _rightProgressBar = Helper.FindChildWithName<Image>(gameObject, "Right Bar");
        gameObject.SetActive(false);
        _instance = this;
    }

    public static void StartKnockdown(int noPresses)
    {
        _instance.gameObject.SetActive(true);
        SkillBar.SetVisible(false);
        CombatManager.Player.PlayerCanvasGroup.alpha = 0.4f;
        InputHandler.UnregisterInputListener(CombatManager.Player);
        SetLastPressDirection(Random.Range(0, 2) == 0 ? -1 : 1);
        _startingPresses = noPresses;
        _pressesRemaining = _startingPresses;
        _instance.StartCoroutine(_instance.CheckForInput());
        SetProgressBarFill(0);
    }

    private static void SetLastPressDirection(int lastDirection)
    {
        _lastPressDirection = lastDirection;
        if (_lastPressDirection == 1)
        {
            _rightText.color = UiAppearanceController.FadedColour;
            _leftText.color = Color.white;
        }
        else
        {
            _leftText.color = UiAppearanceController.FadedColour;
            _rightText.color = Color.white;
        }
    }

    private static void SetProgressBarFill(float fill)
    {
        _leftProgressBar.fillAmount = fill;
        _rightProgressBar.fillAmount = fill;
    }
    
    
    private static void DecreasePresses()
    {
        _pressesRemaining -= 1;
        float fill = 1 - _pressesRemaining / (float)_startingPresses;
        SetProgressBarFill(fill);
    }

    private IEnumerator CheckForInput()
    {
        while (_pressesRemaining > 0)
        {
            float horizontalInput = Input.GetAxis(InputAxis.Horizontal.ToString());
            if (_lastPressDirection < 0 && horizontalInput > 0
                || _lastPressDirection > 0 && horizontalInput < 0)
            {
                SetLastPressDirection(-_lastPressDirection);
                DecreasePresses();
            }

            yield return null;
        }

        Exit();
    }

    public static void Exit()
    {
        SkillBar.SetVisible(true);
        CombatManager.Player.IsKnockedDown = false;
        _instance.gameObject.SetActive(false);
        InputHandler.RegisterInputListener(CombatManager.Player);
        CombatManager.Player.PlayerCanvasGroup.alpha = 1f;
    }
}