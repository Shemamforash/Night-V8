using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Exploration.Ui
{
    public class UIMapController : Menu, IInputListener
    {
        private static EnhancedButton _exploreButton;
        private static UIMapController _instance;
        private List<EnhancedButton> _menuButtons = new List<EnhancedButton>();
        private EnhancedButton _enterButton;
        private EnhancedButton _planButton;
        private UiQuickTravelController _quickTravel;
        private PlayerExplorationController _playerExploration;
        private int _selectedButtonIndex;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.SwitchTab:
                    if (direction > 0) SetSelectedButton(1);
                    else SetSelectedButton(-1);
                    break;
                case InputAxis.Cover:
                    InputHandler.UnregisterInputListener(this);
                    InputHandler.SetCurrentListener(null);
                    SceneManager.LoadScene("Game");
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            GameObject mapOptions = Helper.FindChildWithName(gameObject, "Map Options");
            _enterButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Enter");
            _exploreButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Explore");
            _planButton = Helper.FindChildWithName<EnhancedButton>(mapOptions, "Plan");
            _menuButtons = new List<EnhancedButton>{_enterButton, _exploreButton, _planButton};

            _quickTravel = UiQuickTravelController.Instance;
            _playerExploration = PlayerExplorationController.Instance;
        }

        public void Start()
        {
            Awake();
            if (MapGenerator.AllNodes().FindAll(r => r.Discovered()).Count == 1 || CharacterManager.SelectedCharacter.TravelAction.InTransit())
            {
                _planButton.gameObject.SetActive(false);
                _menuButtons.Remove(_planButton);
            }

            if (CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().GetRegionType() == RegionType.Gate)
            {
                _enterButton.gameObject.SetActive(false);
                _menuButtons.Remove(_enterButton);
            }

            _planButton.AddOnSelectEvent(() => _quickTravel.Enable());
            _planButton.AddOnDeselectEvent(() => _quickTravel.Disable());

            _exploreButton.AddOnSelectEvent(() => _playerExploration.Enable());
            _exploreButton.AddOnDeselectEvent(() => _playerExploration.Disable());

            _enterButton.AddOnClick(() => CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Enter());

            InputHandler.SetCurrentListener(_instance);
            InputHandler.RegisterInputListener(_instance);
            _instance.SetSelectedButton();
        }

        private void SetSelectedButton(int offset = 0)
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
        }
    }
}