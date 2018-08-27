using System;
using System.Collections.Generic;
using DefaultNamespace;
using NUnit.Framework;
using SamsHelper.Input;
using UnityEngine;

public class ListController : MonoBehaviour, IInputListener
{
    private int _selectedItemIndex;
    private int _centreItemIndex;
    private Action<object> OnButtonDown;
    private Action OnReturn;
    private Func<List<object>> GetContentsAction;
    private readonly List<Transform> _listItems = new List<Transform>();
    private List<ListElement> _uiElements = new List<ListElement>();
    private List<object> _listObjects = new List<object>();
    private int _listSize;

    public void Awake()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            _listItems.Add(transform.GetChild(i));
        }

        _centreItemIndex = Mathf.FloorToInt(childCount / 2f);
    }

    public void Initialise(Type elementListType, Action<object> onButtonDown, Action onReturn)
    {
        int listSize = _listItems.Count;
        for (int i = 0; i < listSize; ++i)
        {
            ListElement element = (ListElement) Activator.CreateInstance(elementListType);
            _uiElements.Add(element);
            element.SetElementTransform(_listItems[i]);
            Color elementColour = new Color(1f, 1f, 1f, 1f / (Math.Abs(i - _centreItemIndex) + 1));
            element.SetColour(elementColour);
        }

        OnButtonDown = onButtonDown;
        OnReturn = onReturn;
    }

    public void Initialise(List<ListElement> itemsUi, Action<object> onButtonDown, Action onReturn)
    {
        int itemCount = itemsUi.Count;
        Assert.AreEqual(itemCount, _listItems.Count);
        _uiElements = itemsUi;
        for (int i = 0; i < itemCount; ++i)
        {
            _uiElements[i].SetElementTransform(_listItems[i]);
            Color elementColour = new Color(1f, 1f, 1f, 1f / (Math.Abs(i - _centreItemIndex) + 1));
            _uiElements[i].SetColour(elementColour);
        }

        OnButtonDown = onButtonDown;
        OnReturn = onReturn;
    }

    private bool _inputAllowed;

    public void Show(Func<List<object>> getContentsAction)
    {
        _inputAllowed = false;
        GetContentsAction = getContentsAction;
        gameObject.SetActive(true);
        _selectedItemIndex = 0;
        InputHandler.SetCurrentListener(this);
        UpdateList();
    }

    private void UpdateList()
    {
        _listObjects = GetContentsAction?.Invoke();
        _listSize = _listObjects.Count;
        Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || !_inputAllowed) return;
        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                    TrySelectBelow();
                else
                    TrySelectAbove();
                return;
            case InputAxis.Fire:
                UpdateList();
                OnButtonDown?.Invoke(_listObjects[_selectedItemIndex]);
                break;
            case InputAxis.Cover:
                OnReturn?.Invoke();
                break;
        }
    }

    private void Select()
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

            _uiElements[i].Set(o);
        }
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
        if (axis == InputAxis.Fire)
        {
            _inputAllowed = true;
        }
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}