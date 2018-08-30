using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.Assertions;

public class SkillAnimationController : MonoBehaviour
{
    private static readonly ObjectPool<SkillAnimationController> _skillPool = new ObjectPool<SkillAnimationController>("Skill Animations", "Prefabs/Combat/Effects/Skill Animation");
    private static List<Sprite> _sprites;
    private SpriteRenderer _skillSprite, _glowSprite;
    private float _warmUpTime = 1f;
    private float _glowTime = 1f;
    private float _fadeTime = 2f;
    private Action _callback;

    public void Awake()
    {
        _skillSprite = gameObject.FindChildWithName<SpriteRenderer>("Image");
        _glowSprite = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        Glow();
    }

    public static void Create(string spriteName, float warmUpTime, Action callback)
    {
        SkillAnimationController skillAnimation = _skillPool.Create();
        skillAnimation.Initialise(spriteName, warmUpTime, callback);
    }

    private void Initialise(string spriteName, float warmUpTime, Action callback)
    {
        _callback = callback;
        _warmUpTime = warmUpTime;
        AssignSprite(spriteName);
        Glow();
    }

    private void Glow()
    {
        Color transparentRed = new Color(1f, 0f, 0f, 0f);
        Sequence sequence = DOTween.Sequence();
        _skillSprite.color = UiAppearanceController.InvisibleColour;
        _glowSprite.color = transparentRed;
        sequence.Append(_skillSprite.DOColor(Color.white, _warmUpTime).SetEase(Ease.InExpo));
        sequence.Insert(0f, _glowSprite.DOColor(Color.red, _warmUpTime).SetEase(Ease.InExpo));
        sequence.AppendCallback(() => _callback());
        sequence.AppendInterval(_glowTime);
        sequence.Append(_skillSprite.DOColor(UiAppearanceController.InvisibleColour, _fadeTime).SetEase(Ease.OutQuad));
        sequence.Insert(_warmUpTime + _glowTime, _glowSprite.DOColor(transparentRed, _fadeTime).SetEase(Ease.OutQuad));
        sequence.AppendCallback(() => _skillPool.Return(this));
    }

    public void OnDestroy()
    {
        _skillPool.Dispose(this);
    }
    
    private void AssignSprite(string spriteName)
    {
        if (_sprites == null) _sprites = Resources.LoadAll<Sprite>("Images/Skills").ToList();
        Sprite sprite = _sprites.FirstOrDefault(s => s.name == spriteName);
        Assert.IsNotNull(sprite);
        _skillSprite.sprite = sprite;
    }
}