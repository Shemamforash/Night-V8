using DG.Tweening;
using Game.Characters;
using Game.Combat.Generation;
using UnityEngine;

public class RegionClearController : MonoBehaviour
{
    private static bool _cleared;
    private CanvasGroup _canvasGroup;


    private void Awake()
    {
        _cleared = false;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (_cleared) return;
        if (!CharacterManager.CurrentRegion().IsDynamic()) return;
        if (ContainerController.Containers.Count > 0) return;
        if (CacheController.Active()) return;
        if (RescueRingController.Active()) return;
        if (!CombatManager.Instance().ClearOfEnemies()) return;
        _cleared = true;
        _canvasGroup.DOFade(1f, 1f);
    }

    public static bool Cleared()
    {
        return _cleared;
    }
}