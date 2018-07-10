using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    private static Transform _target;
    private static SpriteRenderer _targetSprite;
    private static bool _locked;

    private void Awake()
    {
        _targetSprite = GetComponent<SpriteRenderer>();
        _targetSprite.color = UiAppearanceController.InvisibleColour;
    }

    public static void SetTarget(Transform target)
    {
        _target = target;
        _targetSprite.color = _target == null ? UiAppearanceController.InvisibleColour : UiAppearanceController.FadedColour;
    }

    public static void SetLocked(bool locked)
    {
        _targetSprite.color = locked ? Color.white : UiAppearanceController.FadedColour;
    }

    public void LateUpdate()
    {
        if (_target == null) return;
        transform.position = _target.transform.position;
    }
}