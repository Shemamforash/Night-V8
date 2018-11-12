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
    private TextMeshProUGUI _text;
    private static ICombatEvent _currentCombatEvent;
    private RadianceController _radianceController;
    private PlayerCombat _player;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    private void Start()
    {
        _radianceController = RadianceController.Instance();
        _player = PlayerCombat.Instance;
    }

    //shelter
    //container
    //shrine
    //outer ring
    //radiance

    private bool CheckForNearbyContainer()
    {
        List<ContainerBehaviour> containers = ContainerController.Containers;
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
        _canvasGroup.DOFade(1, 0.5f);
        _text.text = currentEvent.GetEventText();
    }

    public void Update()
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