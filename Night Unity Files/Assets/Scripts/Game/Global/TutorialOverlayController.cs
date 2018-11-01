using QuickEngine.Extensions;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TutorialOverlayController : MonoBehaviour
{
    private RectTransform _top, _bottom, _left, _right, _topLeft, _topRight, _bottomLeft, _bottomRight, _centre;
    private RectTransform _instructionsTop;
    private VerticalLayoutGroup _verticalGroup;
    private const int TutorialHeight = 250;
    private const int TutorialYOffset = 10;

    public void Awake()
    {
        _top = gameObject.FindChildWithName<RectTransform>("Top");
        _bottom = gameObject.FindChildWithName<RectTransform>("Bottom");
        _left = gameObject.FindChildWithName<RectTransform>("Left");
        _right = gameObject.FindChildWithName<RectTransform>("Right");
        _topLeft = gameObject.FindChildWithName<RectTransform>("Top Left");
        _topRight = gameObject.FindChildWithName<RectTransform>("Top Right");
        _bottomLeft = gameObject.FindChildWithName<RectTransform>("Bottom Left");
        _bottomRight = gameObject.FindChildWithName<RectTransform>("Bottom Right");
        _centre = gameObject.FindChildWithName<RectTransform>("Centre");

        _instructionsTop = gameObject.FindChildWithName<RectTransform>("Instructions Top");
        _verticalGroup = _instructionsTop.GetComponent<VerticalLayoutGroup>();
    }

    public void SetTutorialArea(Vector2 minOffset, Vector2 maxOffset)
    {
        _centre.offsetMin = new Vector2(minOffset.x, maxOffset.y);
        _centre.offsetMax = new Vector2(-maxOffset.x, -minOffset.y);
    }

    private void SetOffset(RectTransform rectTransform, float top, float bottom, float left, float right)
    {
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(-right, top);
    }

    public void Update()
    {
        float _leftOffset = _centre.offsetMin.x;
        float _rightOffset = -_centre.offsetMax.x;
        float _topOffset = _centre.offsetMax.y;
        float _bottomOffset = _centre.offsetMin.y;

        SetOffset(_top, 0, _topOffset, _leftOffset, _rightOffset);
        SetOffset(_bottom, _bottomOffset, 0f, _leftOffset, _rightOffset);
        SetOffset(_left, _topOffset, _bottomOffset, 0f, -_leftOffset);
        SetOffset(_right, _topOffset, _bottomOffset, -_rightOffset, 0f);

        SetOffset(_topLeft, 0f, _topOffset, 0f, -_leftOffset);
        SetOffset(_topRight, 0f, _topOffset, -_rightOffset, 0f);

        SetOffset(_bottomLeft, _bottomOffset, 0f, 0f, -_leftOffset);
        SetOffset(_bottomRight, _bottomOffset, 0f, -_rightOffset, 0f);

        UpdateActiveInstructionCanvas();
    }

    private void UpdateActiveInstructionCanvas()
    {
        Vector3[] v = new Vector3[4];
        _instructionsTop.GetWorldCorners(v);
        bool bottomIsActive = -_centre.offsetMax.y - (TutorialHeight + TutorialYOffset) < 0;

        int yAnchor = bottomIsActive ? 0 : 1;
        _instructionsTop.anchorMin = new Vector2(0.5f, yAnchor);
        _instructionsTop.anchorMax = new Vector2(0.5f, yAnchor);
        _instructionsTop.pivot = new Vector2(0.5f, 1 - yAnchor);

        _verticalGroup.childAlignment = bottomIsActive ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;

        Vector2 position = _instructionsTop.anchoredPosition;
        position.y = bottomIsActive ? -TutorialYOffset : TutorialYOffset;
        _instructionsTop.anchoredPosition = position;
    }
}