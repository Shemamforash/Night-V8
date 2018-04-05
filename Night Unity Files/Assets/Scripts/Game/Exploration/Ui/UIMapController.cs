using System.Collections.Generic;
using Facilitating.UI.Elements;
using SamsHelper;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UIMapController : Menu, IInputListener
{
    private EnhancedButton _enterButton;
    private static EnhancedButton _exploreButton;
    private EnhancedButton _planButton, _returnButton, _cancelButton;
    private readonly List<EnhancedButton> _menuButtons = new List<EnhancedButton>();
    private int _selectedButtonIndex;
    private GameObject _quickTravelObject;
    private static UIMapController _instance;
    
    public void Awake()
    {
        _instance = this;
        GameObject mapOptions = Helper.FindChildWithName(gameObject, "Map Options");
        _enterButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Enter");
        _exploreButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Explore");
        _planButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Plan");
        _returnButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Return");
        _cancelButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Cancel");
        _menuButtons.AddRange(new []{_enterButton, _exploreButton, _planButton, _returnButton, _cancelButton});

        _quickTravelObject = Helper.FindChildWithName(gameObject, "Quick Travel");
        _planButton.AddOnSelectEvent(() => _quickTravelObject.SetActive(true));
        _planButton.AddOnDeselectEvent(() => _quickTravelObject.SetActive(false));
        _quickTravelObject.SetActive(false);
        
        _exploreButton.AddOnSelectEvent(() => CharacterVisionController.Instance().gameObject.SetActive(true));
        _exploreButton.AddOnDeselectEvent(() => CharacterVisionController.Instance().gameObject.SetActive(false));
    }

    public static void Open()
    {
        MenuStateMachine.ShowMenu("Exploration");
        InputHandler.SetCurrentListener(_instance);
        _instance._selectedButtonIndex = 0;
        _instance.SetSelectedButton(0);
    }
    
    private void SetSelectedButton(int offset)
    {
        if (_selectedButtonIndex + offset < 0) return;
        if (_selectedButtonIndex + offset >= _menuButtons.Count) return;
        _selectedButtonIndex += offset;
        for (int i = 0; i < _menuButtons.Count; ++i)
        {
            if (i == _selectedButtonIndex)
            {
                _menuButtons[i].Button().Select();
                _menuButtons[i].transform.Find("Text").GetComponent<EnhancedText>().SetColor(Color.white);
            }
            else
            {
                _menuButtons[i].transform.Find("Text").GetComponent<EnhancedText>().SetColor(UiAppearanceController.FadedColour);
            }
        }

        _menuButtons[_selectedButtonIndex].Button().Select();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.SwitchTab || isHeld) return;
        if (direction > 0)
        {
            SetSelectedButton(1);
            return;
        }

        SetSelectedButton(-1);
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}