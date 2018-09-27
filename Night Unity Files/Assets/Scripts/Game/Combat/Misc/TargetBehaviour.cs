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

    private void UpdateSpriteColour(bool isLocked, CharacterCombat target)
    {
        Color c;
        if (isLocked) c = Color.white;
        else if (target != null) c = UiAppearanceController.FadedColour;
        else c = UiAppearanceController.InvisibleColour;
        _targetSprite.color = c;
    }
    
    public void LateUpdate()
    {
        CharacterCombat currentTarget = PlayerCombat.Instance.GetTarget();
        bool isLocked = PlayerCombat.Instance.IsTargetLocked();
        UpdateSpriteColour(isLocked, currentTarget);
        if (currentTarget == null) return;
        transform.position = currentTarget.transform.position;
    }
}