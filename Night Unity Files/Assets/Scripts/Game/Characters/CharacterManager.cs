using System.Collections.Generic;
using Characters;
using Facilitating.Persistence;
using Facilitating.UI.GameOnly;
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

        private void Awake()
        {
            _persistenceListener = new PersistenceListener(Load, Save, "Character Manager");
            _timeListener.OnHour(UpdateCharacterThirstAndHunger);
        }

        private void UpdateCharacterThirstAndHunger()
        {
            foreach (Character c in _characters)
            {
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

        private void PopulateCharacterUi()
        {
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
                b.onClick.AddListener(delegate { GetComponent<CharacterSelect>().SelectCharacter(b); });
            }
            for (int i = 0; i < _characters.Count; ++i)
            {
                GameObject currentButton = _characters[i].GetCharacterUi().SimpleView;

                if (i == 0)
                {
                    Helper.SetNavigation(currentButton, Home.GetResourceObject(Resource.ResourceType.Water),
                        Helper.NavigationDirections.Up);

                    Helper.SetNavigation(Home.GetResourceObject(Resource.ResourceType.Water),
                        Home.GetResourceObject(Resource.ResourceType.Water),
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(Home.GetResourceObject(Resource.ResourceType.Food), currentButton,
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(Home.GetResourceObject(Resource.ResourceType.Fuel), currentButton,
                        Helper.NavigationDirections.Down);
                    ;
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
            foreach (Character c in _characters)
            {
                if (c.GetCharacterUi().GameObject == g)
                {
                    return c;
                }
            }
            return null;
        }

        private static void ChangeCharacterPanel(GameObject g, bool expand)
        {
            bool foundCharacter = false;
            float moveAmount = 0.3f;
            if (expand)
            {
                moveAmount = -moveAmount;
            }
            for (int i = 0; i < _characters.Count; ++i)
            {
                if (foundCharacter)
                {
                    RectTransform rect = _characters[i].GetCharacterUi().GameObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (_characters[i].GetCharacterUi().GameObject == g)
                {
                    foundCharacter = true;
                    if (expand)
                    {
                        _characters[i].GetCharacterUi().EatButton.Select();
                    }
                }
            }
        }

        public static void ExpandCharacter(GameObject g)
        {
            ChangeCharacterPanel(g, true);
        }

        public static void CollapseCharacter(GameObject g)
        {
            ChangeCharacterPanel(g, false);
        }
    }
}