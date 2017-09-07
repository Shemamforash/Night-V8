using System.Collections.Generic;
using System.Linq;
using Characters;
using Facilitating.Persistence;
using Game.World.Time;
using SamsHelper;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        private static List<Character> _characters = new List<Character>();
        private PersistenceListener _persistenceListener;
        public static Character SelectedCharacter;

        public void Awake()
        {
            _persistenceListener = new PersistenceListener(Load, Save, "Character Manager");
            WorldTime.Instance().HourEvent += UpdateCharacterThirstAndHunger;
            Traits.LoadTraits();
        }

        private void UpdateCharacterThirstAndHunger()
        {
            for (int i = _characters.Count - 1; i >= 0; --i)
            {
                Character c = _characters[i];
                c.Dehydration.Val += c.Thirst / 12f;
                c.Starvation.Val += c.Hunger / 12f;
            }
        }

        private void Load()
        {
        }

        public void Start()
        {
            InputSpeaker.Instance().AddOnPressEvent(InputAxis.Cancel, ExitCharacter);
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
            GameObject inventoryObject = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Inventory");

            float currentY = 1f;
            foreach (Character c in _characters)
            {
                GameObject newCharacterUi = c.CharacterUi.GameObject;
                RectTransform uiRect = newCharacterUi.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, currentY - 0.1f);
                uiRect.anchorMax = new Vector2(1, currentY);
                currentY -= 0.1f;
                newCharacterUi.transform.localScale = new Vector2(1, 1);
                Button b = c.GetCharacterUi().SimpleView.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectCharacter(b); });
            }
            for (int i = 0; i < _characters.Count; ++i)
            {
                GameObject currentButton = _characters[i].GetCharacterUi().SimpleView;

                if (i == 0)
                {
                    Helper.SetNavigation(currentButton, inventoryObject,
                        Helper.NavigationDirections.Up);

                    Helper.SetNavigation(inventoryObject, currentButton,
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
                MenuStateMachine.Instance.NavigateToState("Game Over Menu");
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
            if (SelectedCharacter != null)
            {
                SetDetailedViewActive(false, SelectedCharacter.transform);
                SelectedCharacter = null;
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
            if (SelectedCharacter != null)
            {
                ExitCharacter();
            }
            SelectedCharacter = _characters.FirstOrDefault(c => c.CharacterUi.SimpleView.GetComponent<Button>() == s);
            SetDetailedViewActive(true, s.transform.parent);
        }

        public void SelectActions(Selectable s)
        {
//            _actionSelectable = s;
//            _actionContainer.gameObject.SetActive(true);
//            _actionContainer.GetChild(0).GetComponent<Selectable>().Select();
        }

        public void CharacterEat()
        {
            Character current = FindCharacterFromGameObject(SelectedCharacter.transform.parent.gameObject);
            current.Eat();
        }

        public void CharacterDrink()
        {
            Character current = FindCharacterFromGameObject(SelectedCharacter.transform.parent.gameObject);
            current.Drink();
        }
    }
}