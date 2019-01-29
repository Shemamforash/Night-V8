using System;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class CacheController : MonoBehaviour
{
    private readonly List<CacheButtonController> _orderedButtons = new List<CacheButtonController>();
    private readonly List<CacheButtonController> _scrambledButtons = new List<CacheButtonController>();
    private CacheButtonController _nextButton;
    private static CacheController _instance;
    private static GameObject _prefab;

    private void Awake()
    {
        _instance = this;
        for (int i = 0; i < 5; ++i) AddButton("Anchor " + (i + 1));
        _scrambledButtons.AddRange(_orderedButtons);
        _scrambledButtons.Shuffle();
        _nextButton = _scrambledButtons[0];
    }

    public void Start()
    {
        WorldGrid.FinaliseGrid();
    }
    
    private void OnDestroy()
    {
        _instance = null;
    }

    private void AddButton(string anchorName)
    {
        GameObject anchor = gameObject.FindChildWithName(anchorName);
        CacheButtonController button = anchor.FindChildWithName<CacheButtonController>("Button");
        _orderedButtons.Add(button);
    }

    public static void Generate()
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Cache");
        Instantiate(_prefab, Vector3.zero, Quaternion.identity);
    }

    public static CacheController Instance()
    {
        return _instance;
    }

    public void TryActivateButton(CacheButtonController cacheButtonController)
    {
        if (_nextButton == null) Succeed();
        else if (cacheButtonController == _nextButton) ActivateButton();
        else ResetButtons();
    }

    private void Succeed()
    {
        _scrambledButtons.ForEach(b => b.DisableButton());
    }

    private void ResetButtons()
    {
        _nextButton = _scrambledButtons[0];
        _scrambledButtons.ForEach(b =>
        {
            b.DeactivateButton();
            b.SetEdgeActive(false);
        });
    }

    private void ActivateButton()
    {
        CacheButtonController lastButton = _nextButton;
        _nextButton.ActivateButton();
        int buttonIndex = _scrambledButtons.IndexOf(_nextButton);
        if (buttonIndex + 1 == _scrambledButtons.Count)
        {
            _nextButton = null;
            return;
        }

        _nextButton = _scrambledButtons[buttonIndex + 1];
        GetRelevantEdge(lastButton, _nextButton).SetEdgeActive(true);
    }

    private CacheButtonController GetRelevantEdge(CacheButtonController from, CacheButtonController to)
    {
        int fromIndex = Mathf.Min(_orderedButtons.IndexOf(from), _orderedButtons.IndexOf(to));
        switch (fromIndex)
        {
            case 0:
                return _orderedButtons[1];
            case 1:
                return _orderedButtons[2];
            case 2:
                return _orderedButtons[3];
            case 3:
                return _orderedButtons[4];
            case 4:
                return _orderedButtons[0];
        }

        throw new Exception("No edge found");
    }
}