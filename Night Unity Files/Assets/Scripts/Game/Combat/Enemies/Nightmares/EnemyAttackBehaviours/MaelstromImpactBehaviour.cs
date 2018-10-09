using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class MaelstromImpactBehaviour : MonoBehaviour {

	private static readonly ObjectPool<MaelstromImpactBehaviour> _impactPool = new ObjectPool<MaelstromImpactBehaviour>("Impacts", "Prefabs/Combat/Visuals/Maelstrom Impact");
	
	public static void Create(Vector2 position, Vector2 direction)
	{
		MaelstromImpactBehaviour impact = _impactPool.Create();
		impact.transform.position = position;
		float rot = AdvancedMaths.AngleFromUp(Vector2.zero, direction) - 90;
		impact.transform.rotation = Quaternion.Euler(0, 0, rot);
		impact.StartCoroutine(impact.WaitAndDie());
	}

	private IEnumerator WaitAndDie()
	{
		float timer = 3f;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}
		_impactPool.Return(this);
	}

	private void OnDestroy()
	{
		_impactPool.Dispose(this);
	}
}
