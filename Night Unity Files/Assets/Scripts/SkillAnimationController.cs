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

    public void Awake()
    {
        _skillSprite = gameObject.FindChildWithName<SpriteRenderer>("Image");
        _glowSprite = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        _skillSprite.color = UiAppearanceController.InvisibleColour;
        _glowSprite.color = _transparentRed;
    }

    public static void Create(Transform t, string spriteName, float warmUpTime, Action callback)
    {
        SkillAnimationController skillAnimation = _skillPool.Create();
        skillAnimation.Initialise(t, spriteName, warmUpTime, callback);
    }

    private void Initialise(Transform t, string spriteName, float warmUpTime, Action callback)
    {
        _callback = callback;
        _warmUpTime = warmUpTime;
        _followTransform = t;
        transform.position = t.position;
        AssignSprite(spriteName);
        Glow();
    }

    public void LateUpdate()
    {
        if (_followTransform == null) return;
        transform.position = _followTransform.position;
    }

    private void Glow()
    {
        Sequence sequence = DOTween.Sequence();
        _skillSprite.color = UiAppearanceController.InvisibleColour;
        _glowSprite.color = _transparentRed;
        sequence.Append(_skillSprite.DOColor(Color.white, _warmUpTime).SetEase(Ease.InExpo));
        sequence.Insert(0f, _glowSprite.DOColor(Color.red, _warmUpTime).SetEase(Ease.InExpo));
        sequence.AppendCallback(() =>
        {
            Debug.Log("callback" + _callback);
            _callback?.Invoke();
        });

        sequence.AppendInterval(_glowTime);
        sequence.Append(_skillSprite.DOColor(UiAppearanceController.InvisibleColour, _fadeTime).SetEase(Ease.OutQuad));
        sequence.Insert(_warmUpTime + _glowTime, _glowSprite.DOColor(_transparentRed, _fadeTime).SetEase(Ease.OutQuad));
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