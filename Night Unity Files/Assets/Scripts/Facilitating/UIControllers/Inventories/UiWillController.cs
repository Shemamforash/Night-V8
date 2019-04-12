using DG.Tweening;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiWillController : UiInventoryMenuController
{
    private UIAttributeController _uiAttributeController;
    private EnhancedButton _lifeButton, _gritButton, _focusButton;
    private CanvasGroup _lifeCanvas, _gritCanvas, _focusCanvas, _willCanvas;
    public static bool Locked;

    public override bool Unlocked() => !Locked;

    protected override void CacheElements()
    {
        _uiAttributeController = GetComponent<UIAttributeController>();
        _lifeButton = gameObject.FindChildWithName<EnhancedButton>("Life");
        _gritButton = gameObject.FindChildWithName<EnhancedButton>("Grit");
        _focusButton = gameObject.FindChildWithName<EnhancedButton>("Focus");
        _willCanvas = gameObject.FindChildWithName<CanvasGroup>("Will");
        _lifeCanvas = _lifeButton.GetComponent<CanvasGroup>();
        _gritCanvas = _gritButton.GetComponent<CanvasGroup>();
        _focusCanvas = _focusButton.GetComponent<CanvasGroup>();
    }

    protected override void OnShow()
    {
        UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
        UpdateValues(true);
        _lifeButton.Select();
    }

    protected override void Initialise()
    {
        _lifeButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Life));
        _gritButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Grit));
        _focusButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Focus));
    }

    private void UpdateValues(bool instant)
    {
        float fadeTime = instant ? 0f : 0.5f;
        _uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
        CharacterAttributes attributes = CharacterManager.SelectedCharacter.Attributes;
        float willAlpha = attributes.Get(AttributeType.Will).ReachedMin() ? 0.5f : 1f;
        FadeCanvas(_willCanvas, willAlpha, fadeTime);
        float lifeAlpha = attributes.Get(AttributeType.Life).ReachedMax() ? 0.5f : 1f;
        FadeCanvas(_lifeCanvas, lifeAlpha, fadeTime);
        float gritAlpha = attributes.Get(AttributeType.Grit).ReachedMax() ? 0.5f : 1f;
        FadeCanvas(_gritCanvas, gritAlpha, fadeTime);
        float focusAlpha = attributes.Get(AttributeType.Focus).ReachedMax() ? 0.5f : 1f;
        FadeCanvas(_focusCanvas, focusAlpha, fadeTime);
    }

    private void FadeCanvas(CanvasGroup canvasGroup, float target, float fadeTime)
    {
        canvasGroup.DOFade(target, fadeTime);
    }

    private void TryRestoreAttribute(AttributeType attributeType)
    {
        CharacterAttributes attributes = CharacterManager.SelectedCharacter.Attributes;
        CharacterAttribute will = attributes.Get(AttributeType.Will);
        CharacterAttribute targetAttribute = attributes.Get(attributeType);
        if (targetAttribute.ReachedMax()) return;
        if (will.ReachedMin()) return;
        will.Decrement();
        targetAttribute.Increment();
        UpdateValues(false);
        if (attributeType == AttributeType.Life && PlayerCombat.Instance != null) PlayerCombat.Instance.RecalculateHealth();
        if (attributeType == AttributeType.Focus && PlayerCombat.Instance != null) PlayerCombat.Instance.ResetCompass();
    }
}