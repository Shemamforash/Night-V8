﻿using System.Collections.Generic;
using System.Linq;
using Characters;
using Facilitating.Persistence;
using Menus;
using Persistence;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Game.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        private static List<Character> _characters = new List<Character>();
        private readonly TimeListener _timeListener = new TimeListener();
        private PersistenceListener _persistenceListener;
        private Selectable _actionSelectable;
        private static Selectable _selectedCharacter;
        private Transform _actionContainer;
        private readonly InputListener _inputListener = new InputListener();

        private void Awake()
        {
            _persistenceListener = new PersistenceListener(Load, Save, "Character Manager");
            _timeListener.OnHour(UpdateCharacterThirstAndHunger);
            _inputListener.OnCancel(ExitCharacter);
        }

        private void UpdateCharacterThirstAndHunger()
        {
            for (int i = _characters.Count - 1; i >= 0; --i)
            {
                Character c = _characters[i];
                c.Dehydration.Value += c.Thirst / 12f;
                c.Starvation.Value += c.Hunger / 12f;
            }
        }

        private void Load()
        {
            Traits.LoadTraits();
            ClassCharacter.LoadCharacterClasses();
            if (GameData.Party != null)
            {
                _characters = GameData.Party;
            }
            else
            {
                _characters = CharacterGenerator.LoadInitialParty();
            }
            PopulateCharacterUi();
        }

        private void Save()
        {
            GameData.Party = _characters;
        }

        private static void PopulateCharacterUi()
        {
            GameObject waterObject = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Water").gameObject;
            GameObject foodObject = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Food").gameObject;
            GameObject fuelObject = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Fuel").gameObject;

            float currentY = 1f;
            for (int i = 0; i < _characters.Count; ++i)
            {
                GameObject newCharacterUi = _characters[i].CharacterUi.GameObject;
                RectTransform uiRect = newCharacterUi.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, currentY - 0.1f);
                uiRect.anchorMax = new Vector2(1, currentY);
                currentY -= 0.1f;
                newCharacterUi.transform.localScale = new Vector2(1, 1);
                Button b = _characters[i].GetCharacterUi().SimpleView.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectCharacter(b); });
            }
            for (int i = 0; i < _characters.Count; ++i)
            {
                GameObject currentButton = _characters[i].GetCharacterUi().SimpleView;

                if (i == 0)
                {
                    Helper.SetNavigation(currentButton, waterObject,
                        Helper.NavigationDirections.Up);

                    Helper.SetNavigation(waterObject,currentButton,
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(foodObject, currentButton,
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(fuelObject, currentButton,
                        Helper.NavigationDirections.Down);
                }
                else if (i > 0)
                {
                    GameObject previousButton = _characters[i - 1].GetCharacterUi().SimpleView;
                    Helper.SetNavigation(currentButton, previousButton, Helper.NavigationDirections.Up);
                    Helper.SetNavigation(previousButton, currentButton, Helper.NavigationDirections.Down);
                }
            }
        }

        public static Character FindCharacterFromGameObject(GameObject g)
        {
            return _characters.FirstOrDefault(c => c.GetCharacterUi().GameObject == g);
        }

        public static void RemoveCharacter(Character c, bool isDriver)
        {
            _characters.Remove(c);
            PopulateCharacterUi();
            if (isDriver)
            {
                WorldState.MenuNavigator.EndGameFail();
            }
        }

        private static void ChangeCharacterPanel(GameObject g, bool expand)
        {
            bool foundCharacter = false;
            float moveAmount = 0.3f;
            if (expand)
            {
                moveAmount = -moveAmount;
            }
            foreach (Character c in _characters)
            {
                if (foundCharacter)
                {
                    RectTransform rect = c.GetCharacterUi().GameObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (c.GetCharacterUi().GameObject == g)
                {
                    foundCharacter = true;
                    CharacterUI foundUi = c.GetCharacterUi();
                    if (expand)
                    {
                        foundUi.SwitchToDetailedView();
                    }
                    else
                    {
                        foundUi.SwitchToSimpleView();
                    }
                }
            }
        }

        public static void ExitCharacter()
        {
            if (_selectedCharacter != null)
            {
                SetDetailedViewActive(false, _selectedCharacter.transform.parent);
                _selectedCharacter = null;
            }
        }

        private static void SetDetailedViewActive(bool active, Transform characterUiObject)
        {
            if (active)
            {
                ChangeCharacterPanel(characterUiObject.gameObject, true);
            }
            else
            {
                ChangeCharacterPanel(characterUiObject.gameObject, false);
            }
        }

        public static void SelectCharacter(Selectable s)
        {
            if (_selectedCharacter != null)
            {
                ExitCharacter();
            }
            _selectedCharacter = s;
            SetDetailedViewActive(true, s.transform.parent);
        }

        public void SelectActions(Selectable s)
        {
            _actionSelectable = s;
            _actionContainer.gameObject.SetActive(true);
            _actionContainer.GetChild(0).GetComponent<Selectable>().Select();
        }
        
        public void CharacterEat()
        {
            Character current = FindCharacterFromGameObject(_selectedCharacter.transform.parent.gameObject);
            current.Eat();
        }

        public void CharacterDrink()
        {
            Character current = FindCharacterFromGameObject(_selectedCharacter.transform.parent.gameObject);
            current.Drink();
        }
    }
}