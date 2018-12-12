using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

public class EventTextController : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _eventText;
    private static ICombatEvent _currentCombatEvent;
    private RadianceController _radianceController;
    private PlayerCombat _player;
    private bool _overridingText;
    private static EventTextController _instance;
    private Sequence _revealSequence;
    private bool _seenRegionClearText;

    private void Awake()
    {
        _instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _eventText = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Start()
    {
        _radianceController = RadianceController.Instance();
        _player = PlayerCombat.Instance;
    }

    private bool CheckRegionClear()
    {
        if (_seenRegionClearText) return false;
        if (!CombatManager.GetCurrentRegion().IsDynamic()) return false;
        if (ContainerController.Containers.Count > 0) return false;
        if (!CombatManager.ClearOfEnemies()) return false;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(3f);
        sequence.AppendCallback(HideOverride);
        _seenRegionClearText = true;
        SetOverrideText("Region cleared");
        return true;
    }

    private bool CheckForNearbyContainer()
    {
        List<ContainerBehaviour> containers = ContainerController.Containers;
        if (CheckRegionClear()) return true;
        ContainerBehaviour nearestContainer = null;
        float nearestContainerDistance = 10000;
        containers.ForEach(c =>
        {
            float distance = c.InRange();
            if (distance < 0) return;
            if (distance >= nearestContainerDistance) return;
            nearestContainer = c;
            nearestContainerDistance = distance;
        });
        if (nearestContainer == null) return false;
        SetCurrentCombatEvent(nearestContainer);
        return true;
    }

    public static void Activate()
    {
        _currentCombatEvent?.Activate();
    }

    private void SetCurrentCombatEvent(ICombatEvent currentEvent)
    {
        if (currentEvent == _currentCombatEvent) return;
        _currentCombatEvent = currentEvent;
        _eventText.text = currentEvent.GetEventText();
        _revealSequence?.Kill();
        _revealSequence = DOTween.Sequence();
        _revealSequence.Append(_canvasGroup.DOFade(1, 0.5f));
    }

    //shelter
    //container
    //shrine
    //outer ring
    //radiance
    private void DoCheckForPointsOfInterest()
    {
        if (CheckForNearbyShelterCharacter()) return;
        if (CheckForNearbyContainer()) return;
        if (CheckForNearbyShrines()) return;
        if (CheckForOuterRing()) return;
        if (CheckForRadiance()) return;
        _currentCombatEvent = null;
        if (_canvasGroup.alpha == 0) return;
        _canvasGroup.DOFade(0, 0.5f);
    }

    public void Update()
    {
        if (_overridingText) return;
        DoCheckForPointsOfInterest();
    }

    public static void CloseOverrideText()
    {
        _instance.HideOverride();
    }

    public static void SetOverrideText(string overrideString)
    {
        _instance._eventText.text = overrideString;
        _instance.ShowOverride();
    }

    private void HideOverride()
    {
        _revealSequence?.Kill();
        _revealSequence = DOTween.Sequence();
        _revealSequence.Append(_canvasGroup.DOFade(0f, 1f));
        _revealSequence.AppendCallback(() => _overridingText = false);
    }

    private void ShowOverride()
    {
        _revealSequence?.Kill();
        _revealSequence = DOTween.Sequence();
        _revealSequence.AppendCallback(() => _overridingText = true);
        _revealSequence.Append(_canvasGroup.DOFade(1f, 1f));
    }

    private bool CheckForNearbyShelterCharacter()
    {
        ShelterCharacterBehaviour shelterCharacter = ShelterCharacterBehaviour.Instance();
        if (shelterCharacter == null) return false;
        if (shelterCharacter.InRange() > 1.5f) return false;
        if (!shelterCharacter.ShowText()) return false;
        SetCurrentCombatEvent(shelterCharacter);
        return true;
    }

    private bool CheckForRadiance()
    {
        if (_radianceController.InRange() < 0) return false;
        SetCurrentCombatEvent(_radianceController);
        return true;
    }

    private bool CheckForOuterRing()
    {
        if (_player == null) return false;
        if (_player.InRange() < 0) return false;
        SetCurrentCombatEvent(_player);
        return true;
    }

    private bool CheckForNearbyShrines()
    {
        ICombatEvent shrine = FountainBehaviour.Instance();
        if (shrine == null) shrine = RiteShrineBehaviour.Instance();
        if (shrine == null) shrine = SaveStoneBehaviour.Instance();
        if (shrine == null) return false;
        if (shrine.InRange() < 0) return false;
        SetCurrentCombatEvent(shrine);
        return true;
    }
}