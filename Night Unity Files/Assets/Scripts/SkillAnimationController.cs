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
    private Transform _followTransform;
    private static readonly Color _transparentRed = new Color(1f, 0f, 0f, 0f);
    private Sequence _glowSequence;
    private bool _fading;

    public void Awake()
    {
        _skillSprite = gameObject.FindChildWithName<SpriteRenderer>("Image");
        _glowSprite = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        _skillSprite.color = UiAppearanceController.InvisibleColour;
        _glowSprite.color = _transparentRed;
    }

    public static void Create(Transform t, string spriteName, float warmUpTime, Action callback, float cooldownTime = 2f)
    {
        SkillAnimationController skillAnimation = _skillPool.Create();
        skillAnimation.Initialise(t, spriteName, warmUpTime, callback, cooldownTime);
    }

    private void Initialise(Transform t, string spriteName, float warmUpTime, Action callback, float cooldownTime)
    {
        _callback = callback;
        _warmUpTime = warmUpTime;
        _fadeTime = cooldownTime;
        _followTransform = t;
        transform.position = t.position;
        AssignSprite(spriteName);
        Glow();
    }

    public void LateUpdate()
    {
        if (_followTransform == null)
        {
            if (_fading) return;
            _glowSequence.Kill();
            _glowSequence = DOTween.Sequence();
            _glowSequence.Append(_skillSprite.DOColor(UiAppearanceController.InvisibleColour, _fadeTime).SetEase(Ease.OutQuad));
            _glowSequence.Insert(_warmUpTime + _glowTime, _glowSprite.DOColor(_transparentRed, _fadeTime).SetEase(Ease.OutQuad));
            _glowSequence.AppendCallback(() => _skillPool.Return(this));
            _fading = true;
            return;
        }

        transform.position = _followTransform.position;
    }

    private void Glow()
    {
        _glowSequence = DOTween.Sequence();
        _skillSprite.color = UiAppearanceController.InvisibleColour;
        _glowSprite.color = _transparentRed;
        _glowSequence.Append(_skillSprite.DOColor(Color.white, _warmUpTime).SetEase(Ease.InExpo));
        _glowSequence.Insert(0f, _glowSprite.DOColor(Color.red, _warmUpTime).SetEase(Ease.InExpo));
        _glowSequence.AppendCallback(() => { _callback?.Invoke(); });
        _glowSequence.AppendInterval(_glowTime);
        _glowSequence.Append(_skillSprite.DOColor(UiAppearanceController.InvisibleColour, _fadeTime).SetEase(Ease.OutQuad));
        _glowSequence.Insert(_warmUpTime + _glowTime, _glowSprite.DOColor(_transparentRed, _fadeTime).SetEase(Ease.OutQuad));
        _glowSequence.AppendCallback(() => _skillPool.Return(this));
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