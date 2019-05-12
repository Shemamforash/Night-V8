using DG.Tweening;
using Game.Characters;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using UnityEngine;

public class RegionClearController : MonoBehaviour
{
	private static bool        _cleared;
	private        CanvasGroup _canvasGroup;

	private void Awake()
	{
		_cleared           = false;
		_canvasGroup       = GetComponent<CanvasGroup>();
		_canvasGroup.alpha = 0;
	}

	private void Update()
	{
		if (_cleared) return;
		Region currentRegion = CharacterManager.CurrentRegion();
		bool   isTemple      = currentRegion.GetRegionType() == RegionType.Temple;
		bool   isDynamic     = currentRegion.IsDynamic();
		if (!isTemple && !isDynamic) return;
		if (isTemple  && currentRegion.IsTempleCleansed() == false) return;
		if (ContainerController.Containers.Count > 0) return;
		if (CacheController.Active()) return;
		if (RescueRingController.Active()) return;
		if (!CombatManager.Instance().ClearOfEnemies()) return;
		_cleared = true;
		currentRegion.SetCleared();
		_canvasGroup.DOFade(1f, 1f);
	}

	public static bool Cleared() => _cleared;
}