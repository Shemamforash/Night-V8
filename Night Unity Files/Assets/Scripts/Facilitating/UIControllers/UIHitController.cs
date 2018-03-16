using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIHitController : MonoBehaviour
{
    private RectTransform _innerRect;
    private Image _outerImage, _innerImage;
    private const float MaxHeight = 70f;
    private CharacterCombat _character;
    private const float FadeTime = 0.5f;
    private float _currentShotTime;
    private float _currentCriticalTime;

    public void Awake()
    {
        _innerRect = Helper.FindChildWithName<RectTransform>(gameObject, "Inner");
        _innerImage = _innerRect.GetComponent<Image>();
        _outerImage = Helper.FindChildWithName<Image>(gameObject, "Outer");
    }

    public void SetCharacter(CharacterCombat character)
    {
        _character = character;
    }
    
    public void Update()
    {
        float distance = _character.DistanceToTarget();
        distance = 1f / distance;
        if (distance > 1) distance = 1;
        float newHeight = MaxHeight * (CombatManager.Player.RecoilManager.GetAccuracyModifier() * 2f - 1f) * distance;
        if (_currentShotTime > 0)
        {
            float rValue = 1 - _currentShotTime / FadeTime;
            _innerImage.color = new Color(1, rValue, rValue, 1);
            _currentShotTime -= Time.deltaTime;
        }

        if (_currentCriticalTime > 0)
        {
            float rValue = 1 - _currentCriticalTime / FadeTime;
            _outerImage.color = new Color(1, rValue, rValue, 1);
            _currentCriticalTime -= Time.deltaTime;
        }

        _innerRect.sizeDelta = new Vector2(newHeight, newHeight);
    }

    public void RegisterCritical()
    {
        _currentCriticalTime = FadeTime;
        _currentShotTime = FadeTime;
    }

    public void RegisterShot()
    {
        _currentShotTime = FadeTime;
    }
}