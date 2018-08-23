using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Exploration.Environment;
using UnityEngine;

public class TombPortalBehaviour : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other)
	{
		switch (EnvironmentManager.CurrentEnvironment.LevelNo)
		{
			case 0:
//                    OvaBehaviour.Create();
				Sequence sequence = DOTween.Sequence();
				sequence.AppendInterval(5f);
				sequence.Append(transform.DOScale(0f, 1f).SetEase(Ease.InCubic));
				SerpentBehaviour.Create();
				break;
			case 1:
				StarfishBehaviour.Create();
				break;
			case 2:
				SwarmBehaviour.Create();
				break;
			case 3:
				OvaBehaviour.Create();
				break;
			case 4:
				break;
		}
		Destroy(this);
	}
}
