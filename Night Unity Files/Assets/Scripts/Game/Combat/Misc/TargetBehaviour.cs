using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    private static SpriteRenderer _targetSprite;

    private void Awake()
    {
        _targetSprite = GetComponent<SpriteRenderer>();
        _targetSprite.color = UiAppearanceController.InvisibleColour;
    }

    private void UpdateSpriteColour(bool isLocked, CanTakeDamage target)
    {
        Color c;
        if (isLocked) c = Color.white;
        else if (target != null) c = UiAppearanceController.FadedColour;
        else c = UiAppearanceController.InvisibleColour;
        _targetSprite.color = c;
    }
    
    public void LateUpdate()
    {
        CanTakeDamage currentTarget = PlayerCombat.Instance.GetTarget();
        currentTarget = null;
//        bool isLocked = PlayerCombat.Instance.IsTargetLocked();
        UpdateSpriteColour(false, currentTarget);
        if (currentTarget == null) return;
        transform.position = PlayerCombat.Instance.TargetPosition();
    }
}