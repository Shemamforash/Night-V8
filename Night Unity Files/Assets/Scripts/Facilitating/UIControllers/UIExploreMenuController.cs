﻿using System.Collections.Generic;
using Game.Characters;
using Game.World.Region;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.UIControllers
{
    public class UIExploreMenuController : Menu
    {
        private TextMeshProUGUI _titleText;
        private GameObject _regionContainer;
        private readonly List<GameObject> _regionUiList = new List<GameObject>();
        private EnhancedButton _lookAroundButton, _returnButton;
        private Player _player;
        private Region _currentRegion;
        private static UIExploreMenuController _instance;

        public void Awake()
        {
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Title");
            _regionContainer = Helper.FindChildWithName(gameObject, "Region Container");
            _lookAroundButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Look");
            _returnButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Return");
            _instance = this;
        }

        public void SetRegion(Region region, Player player)
        {
            _player = player;
            _currentRegion = region;
            bool isInitialRegion = _currentRegion.Origin == null;
            _lookAroundButton.gameObject.SetActive(!isInitialRegion);
            _titleText.text = isInitialRegion ? "Setting Out" : _currentRegion.Name;
            foreach (Region connection in region.Connections)
            {
                GameObject regionUi = Helper.InstantiateUiObject("Prefabs/Region Explore UI", _regionContainer.transform);
                UIRegionItem regionUiController = regionUi.GetComponent<UIRegionItem>();
                regionUiController.SetText("left text", connection.Name, "right text");
                regionUiController.SetRegion(connection, _player, CloseMenu);
                _regionUiList.Add(regionUi);
            }

            for (int i = 0; i < _regionUiList.Count; ++i)
            {
                EnhancedButton currentButton = _regionUiList[i].GetComponent<EnhancedButton>();
                EnhancedButton nextButton;
                if (i < region.Connections.Count - 1)
                {
                    nextButton = _regionUiList[i + 1].GetComponent<EnhancedButton>();
                }
                else if (_lookAroundButton.gameObject.activeInHierarchy)
                {
                    nextButton = _lookAroundButton;
                    _returnButton.SetUpNavigation(nextButton);
                    nextButton.SetDownNavigation(currentButton);
                }
                else
                {
                    nextButton = _returnButton.GetComponent<EnhancedButton>();
                }
                currentButton.SetDownNavigation(nextButton);
                nextButton.SetUpNavigation(currentButton);
            }

            _regionUiList[0].GetComponent<Button>().Select();
            MenuStateMachine.ShowMenu("Region Explore Menu");
        }

        public static UIExploreMenuController Instance()
        {
            return _instance;
        }

        public void LookAround()
        {
            _currentRegion?.Enter(_player);
        }

        public void CloseAndReturn()
        {
            _player.ReturnAction.Enter();
            CloseMenu();
        }

        private void CloseMenu()
        {
            _regionUiList.ForEach(Destroy);
            _regionUiList.Clear();
            MenuStateMachine.GoToInitialMenu();
        }
    }
}