using System;
using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers;
using NUnit.Framework;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class ListController : MonoBehaviour, IInputListener
{
    private int _selectedItemIndex;
    private int _centreItemIndex;
    private Action<object> OnItemHover;
    private Action OnReturn;
    private Func<List<object>> GetContentsAction;
    private readonly List<Transform> _listItems = new List<Transform>();
    private List<ListElement> _uiElements = new List<ListElement>();
    private List<object> _listObjects = new List<object>();
    private int _listSize;
    private EnhancedButton _centreButton;
    private float _skipTime;
    private float _currentScrollAmount;
    private const float SkipTimeMax = 0.25f;

    private void CacheElements()
    {
        _listItems.Clear();
        int childCount = transform.childCount;
        _centreItemIndex = Mathf.FloorToInt(childCount / 2f);

        for (int i = 0; i < childCount; ++i)
        {
            _listItems.Add(transform.GetChild(i));
            if (i != _centreItemIndex) continue;
            _centreButton = _listItems[i].FindChildWithName<EnhancedButton>("Button");
        }
    }

    public void SetOnItemHover(Action<object> onItemHover)
    {
        OnItemHover = onItemHover;
    }

    public void Initialise(Type elementListType, Action<object> onButtonDown, Action onReturn)
    {
        CacheElements();
        int listSize = _listItems.Count;
        for (int i = 0; i < listSize; ++i)
        {
            ListElement element = (ListElement) Activator.CreateInstance(elementListType);
            _uiElements.Add(element);
            element.SetElementTransform(_listItems[i]);
            float opacityDivider = Mathf.Abs(i - _centreItemIndex) + 1f;
            Color elementColour = new Color(1f, 1f, 1f, 1f / opacityDivider);
            element.SetColour(elementColour);
        }

        _centreButton.AddOnClick(() =>
        {
            if (_listObjects.Count == 0) return;
            if (_selectedItemIndex == _listObjects.Count) --_selectedItemIndex;
            onButtonDown?.Invoke(_listObjects[_selectedItemIndex]);
            UpdateList();
        });
        OnReturn = onReturn;
    }

    public void Initialise(List<ListElement> itemsUi, Action<object> onButtonDown, Action onReturn)
    {
        CacheElements();
        int itemCount = itemsUi.Count;
        Assert.AreEqual(itemCount, _listItems.Count);
        _uiElements = itemsUi;
        for (int i = 0; i < itemCount; ++i)
        {
            _uiElements[i].SetElementTransform(_listItems[i]);
            Color elementColour = new Color(1f, 1f, 1f, 1f / (Math.Abs(i - _centreItemIndex) + 1));
            _uiElements[i].SetColour(elementColour);
        }

        _centreButton.AddOnClick(() =>
        {
            onButtonDown?.Invoke(_listObjects[_selectedItemIndex]);
            UpdateList();
        });
        OnReturn = onReturn;
    }

    public void Show(Func<List<object>> getContentsAction)
    {
        GetContentsAction = getContentsAction;
        gameObject.SetActive(true);
        _centreButton.Select();
        _selectedItemIndex = 0;
        InputHandler.SetCurrentListener(this);
        UpdateList(false);
    }

    private void UpdateList(bool playSound = true)
    {
        _listObjects = GetContentsAction.Invoke();
        _listSize = _listObjects.Count;
        Select(playSound);
    }

    public void Update()
    {
        if (!Cursor.visible) return;
        if (InputHandler.GetCurrentListener() != this)
        {
            _currentScrollAmount = 0f;
            return;
        }

        float mouseDelta = Input.mouseScrollDelta.y;
        if (mouseDelta == 0) return;
        if (mouseDelta < 0 && _currentScrollAmount > 0) _currentScrollAmount = 0;
        else if (mouseDelta > 0 && _currentScrollAmount < 0) _currentScrollAmount = 0;
        _currentScrollAmount += mouseDelta;
        if (_currentScrollAmount <= -1)
        {
            TrySelectBelow();
            _currentScrollAmount = 0f;
        }
        else if (_currentScrollAmount >= 1)
        {
            TrySelectAbove();
            _currentScrollAmount = 0f;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld)
        {
            if (axis != InputAxis.Vertical) return;
            _skipTime += Time.unscaledDeltaTime;
            if (_skipTime < SkipTimeMax) return;
            _skipTime = SkipTimeMax;
        }
        else
        {
            _skipTime = 0f;
        }

        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                    TrySelectBelow();
                else
                    TrySelectAbove();
                return;
            case InputAxis.Cover:
                OnReturn?.Invoke();
                break;
        }
    }

    private void Select(bool playSound = true)
    {
        for (int i = 0; i < _listItems.Count; ++i)
        {
            int offset = i - _centreItemIndex;
            int objectIndex = _selectedItemIndex + offset;
            object o = null;
            if (objectIndex >= 0 && objectIndex < _listSize)
            {
                o = _listObjects[objectIndex];
            }

            bool isCentreItem = i == _centreItemIndex;
            _uiElements[i].Set(o, isCentreItem);
            if (isCentreItem && _listObjects.Count > 0) OnItemHover?.Invoke(_listObjects[_selectedItemIndex]);
        }

        if (!playSound) return;
        ButtonClickListener.Click();
    }

    private void TrySelectBelow()
    {
        if (_selectedItemIndex == _listObjects.Count - 1) return;
        ++_selectedItemIndex;
        Select();
    }

    private void TrySelectAbove()
    {
        if (_selectedItemIndex == 0) return;
        --_selectedItemIndex;
        Select();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}