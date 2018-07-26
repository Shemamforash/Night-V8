using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;

public class InsectBehaviour : MonoBehaviour {
	public void Awake()
	{
		GetNewPath();
	}

	private void GetNewPath()
	{
		int numberOfPoints = Random.Range(1, 5);
		Vector3[] path = new Vector3[numberOfPoints + 1];
		path[0] = transform.up * 0.2f + transform.localPosition;
		float distance = 0f;
		for (int i = 0; i < numberOfPoints; ++i)
		{
			path[i + 1] = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 0.7f);
			distance += Vector2.Distance(path[i], path[i + 1]);
		}
		float duration = distance * Random.Range(0.8f, 1.2f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(transform.DOLocalPath(path, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear));
		sequence.AppendCallback(GetNewPath);
	}
}
