using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;

public class ShrineEdgeFollowBehaviour : MonoBehaviour
{
	public PolygonCollider2D _polyToFollow;
	private List<Vector2> _vertices = new List<Vector2>();
	public int StartVertex;
	private float Speed;

	public void Awake()
	{
		_vertices = _polyToFollow.points.ToList();
		StartVertex = Random.Range(0, _vertices.Count);
		Speed = Random.Range(0.5f, 1f);
		if (Random.Range(0, 2) == 0) Speed = -Speed;
		StartCoroutine(FollowPoly());
	}

	private IEnumerator FollowPoly()
	{
		int nextVertexIndex = _vertices.NextIndex(StartVertex);
		Vector2 nextVertex = _vertices[nextVertexIndex];
		float distance = Vector2.Distance(_vertices[StartVertex], nextVertex);
		float time = distance / Speed;
		StartVertex = nextVertexIndex;
		yield return transform.DOLocalMove(nextVertex, time).SetEase(Ease.Linear).WaitForCompletion();
		StartCoroutine(FollowPoly());
	}
}
