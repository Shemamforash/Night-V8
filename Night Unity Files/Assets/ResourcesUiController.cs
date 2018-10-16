using DG.Tweening;
using UnityEngine;

public class ResourcesUiController : MonoBehaviour
{

	private static CanvasGroup _canvas;

	public void Awake()
	{
		_canvas = GetComponent<CanvasGroup>();
	}

	public static void Show()
	{
		_canvas.DOFade(1f, 1f);
	}

	public static void Hide()
	{
		_canvas.DOFade(0f, 1f);
	}
}
