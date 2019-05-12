using DG.Tweening;
using UnityEngine;

public class SunBehaviour : MonoBehaviour
{
	private const float SecondSunRadius = 120;
	private const float SecondSunHeight = 50;
	private const float ThirdSunRadius  = 180;
	private const float ThirdSunHeight  = 75;

	private void StartOrbit(string sunName, float radius, float height, float time)
	{
		RectTransform sunTransform = transform.Find(sunName).GetComponent<RectTransform>();
		sunTransform.anchoredPosition = new Vector2(radius, height);
		Canvas   sunCanvas   = sunTransform.GetComponent<Canvas>();
		Sequence sunSequence = DOTween.Sequence();
		sunSequence.Append(sunTransform.DOAnchorPos(new Vector2(-radius, -height), time).SetEase(Ease.InOutSine));
		sunTransform.DOAnchorPos(new Vector2(-radius, -height), time).SetEase(Ease.InOutSine);
		sunSequence.InsertCallback(time, () => sunCanvas.sortingOrder = sunCanvas.sortingOrder == -16 ? -14 : -16);
		sunSequence.SetLoops(-1, LoopType.Yoyo);
	}

	public void Awake()
	{
		StartOrbit("Second Sun", SecondSunRadius, SecondSunHeight, 60f);
		StartOrbit("Third Sun",  -ThirdSunRadius, -ThirdSunHeight, 180f);
	}
}