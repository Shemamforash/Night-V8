using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    private static Transform _target;
    private SpriteRenderer _targetSprite;

    private void Awake()
    {
        _targetSprite = GetComponent<SpriteRenderer>();
    }

    public static void SetTarget(Transform target)
    {
        _target = target;
    }

    public void LateUpdate()
    {
        if (_target == null)
        {
            _targetSprite.color = UiAppearanceController.InvisibleColour;
        }
        else
        {
            _targetSprite.color = Color.white;
            transform.position = _target.transform.position;
        }
    }
}