using DG.Tweening;
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
        if (!CombatManager.GetCurrentRegion().IsDynamic()) return;
        if (ContainerController.Containers.Count > 0) return;
        if (!CombatManager.ClearOfEnemies()) return;
        _cleared = true;
        _canvasGroup.DOFade(1f, 1f);
    }

    public static bool Cleared()
    {
        return _cleared;
    }
}