using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.World.Time;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Facilitating.MenuNavigation
{
    public class Popup
    {
        private static string _optionPrefabName = "Prefabs/Button Border";
        private readonly Transform _optionContainer;
        private readonly GameObject _popupObject;
        private readonly List<GameObject> _options = new List<GameObject>();
        private readonly GameObject _previousSelectable;

        public Popup(string title)
        {
            WorldTime.Instance().Pause();
            _popupObject = Helper.InstantiateUiObject("Prefabs/Popup Menu", GameObject.Find("Canvas").transform);
            Helper.FindChildWithName<TextMeshProUGUI>(_popupObject, "Title").text = title;
            _optionContainer = _popupObject.transform.Find("Bar");
            _previousSelectable = EventSystem.current.currentSelectedGameObject;
        }

        public Popup(string title, string optionPrefabName) : this(title)
        {
            _optionPrefabName = optionPrefabName;
        }

        private void Destroy()
        {
            WorldTime.Instance().UnPause();
            GameObject.Destroy(_popupObject);
            _previousSelectable.GetComponent<Selectable>().Select();
        }

        public void AddOption(string optionText = "Cancel", Action optionOnClick = null)
        {
            GameObject newOption = Helper.InstantiateUiObject(_optionPrefabName, _optionContainer);
            Button b = newOption.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                Destroy();
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
                Helper.SetReciprocalNavigation(_options[i - 1], newOption);
            }
        }
    }
}