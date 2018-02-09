using SamsHelper;
using UnityEngine;

public class UIRecoilManager : MonoBehaviour
{
	private RectTransform _leftContainer, _rightContainer;
	private RectTransform _barLeft, _barRight;

	private void Awake()
	{
		_leftContainer = Helper.FindChildWithName<RectTransform>(gameObject, "Left Recoil");
		_rightContainer = Helper.FindChildWithName<RectTransform>(gameObject, "Right Recoil");
		_barLeft = _leftContainer.Find("Bar").GetComponent<RectTransform>();
		_barRight = _rightContainer.Find("Bar").GetComponent<RectTransform>();
	}

	public void SetValue(float normalisedValue)
	{
		float rectWidth = _leftContainer.rect.width;
		float offset = rectWidth * normalisedValue;
		_barLeft.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, offset, 2);
		_barRight.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, offset, 2);
	}
}
