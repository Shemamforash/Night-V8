using System;
using System.Collections.Generic;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Facilitating.MenuNavigation
{
    public class Popup
    {
        private readonly GameObject _popupObject;
        private readonly Transform _container;
        private readonly GameObject _previousSelectable;
        private static string _optionPrefabName = "Prefabs/Button Border";
        private readonly List<GameObject> _options = new List<GameObject>();
        private static readonly Stack<Popup> PopupStack = new Stack<Popup>();
        private readonly Popup _previousPopup;

        public Popup(string title)
        {
            _previousPopup = PopupStack.Count == 0 ? null : PopupStack.Peek();
            if (_previousPopup == null)
            {
                MenuStateMachine.HideCurrentMenu();
                WorldState.Pause();
            }
            else
            {
                _previousPopup.Hide();
            }
            _popupObject = Helper.InstantiateUiObject("Prefabs/Popup Menu", GameObject.Find("Canvas").transform);
            _container = _popupObject.transform.Find("Bar");
            _previousSelectable = EventSystem.current.currentSelectedGameObject;
            Helper.FindChildWithName<TextMeshProUGUI>(_popupObject, "Title").text = title;
            PopupStack.Push(this);
        }

        private void Destroy(bool clearStack = false)
        {
            GameObject.Destroy(_popupObject);
            _previousSelectable.GetComponent<Selectable>().Select();
            Popup thisPopup = PopupStack.Pop();
            if (thisPopup != this)
            {
                throw new Exception("Next popup in popup stack should have been this element, but ,uh, it wasn't.");
            }
            if (_previousPopup == null)
            {
                MenuStateMachine.ShowCurrentMenu();
                WorldState.UnPause();
            }
            else
            {
                if (clearStack)
                {
                    _previousPopup.Destroy(true);
                }
                _previousPopup.Show();
            }
        }

        public void AddBackButton(string title = "Cancel")
        {
            AddButton(title, null, true);
        }

        public void AddList(List<MyGameObject> items, Action<MyGameObject> callback, bool centered = false, bool autoDestruct = false, bool closeAll = false)
        {
            GameObject menuObject = Helper.InstantiateUiObject("Prefabs/Simple Menu", _container);
            MenuList menuList = centered ? menuObject.AddComponent<ScrollingMenuList>() : menuObject.AddComponent<MenuList>();
            menuList.SetItems(items);
            menuList.GetItems().ForEach(item =>
            {
                _options.Add(item.GetGameObject());
                item.OnPress(() =>
                {
                    if(autoDestruct) Destroy(closeAll);
                    callback?.Invoke(item.GetLinkedObject());
                });
            });
        }

        public void AddButton(string optionText, Action optionOnClick = null, bool autoDestruct = false, bool closeAll = false)
        {
            GameObject newOption = Helper.InstantiateUiObject(_optionPrefabName, _container);
            Button b = newOption.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                if(autoDestruct) Destroy(closeAll);
                optionOnClick?.Invoke();
            });
            if (_options.Count == 0)
            {
                b.Select();
            }
            Helper.FindChildWithName<TextMeshProUGUI>(newOption, "Text").text = optionText;
            _options.Add(newOption);
            int i = _options.IndexOf(newOption);
            if (i > 0)
            {
                Helper.SetReciprocalNavigation(newOption, _options[0]);
                Helper.SetReciprocalNavigation(_options[i - 1], newOption);
            }
            b.Select();
        }

        private void Hide()
        {
            _popupObject.SetActive(false);
        }

        private void Show()
        {
            _popupObject.SetActive(true);
        }
    }
}