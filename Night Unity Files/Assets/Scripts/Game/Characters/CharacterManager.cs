using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Characters;
using Facilitating.MenuNavigation;
using Facilitating.Persistence;
using Game.World;
using SamsHelper;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterManager : MonoBehaviour , IPersistenceTemplate
    {
        private static List<DesolationCharacter> _characters = new List<DesolationCharacter>();
        public static DesolationCharacter SelectedCharacter;

        public void Awake()
        {
            Traits.LoadTraits();
            SaveController.AddPersistenceListener(this);
        }

        public void Start()
        {
            InputSpeaker.Instance().AddOnPressEvent(InputAxis.Cancel, ExitCharacter);
            if (_characters.Count == 0)
            {
                _characters = CharacterGenerator.LoadInitialParty();
            }
            PopulateCharacterUi();
        }

        private static void PopulateCharacterUi()
        {
            GameObject inventoryObject = WorldState.GetInventoryButton();

            float currentY = 1f;
            foreach (DesolationCharacter c in _characters)
            {
                GameObject newCharacterUi = c.CharacterUi.GameObject;
                RectTransform uiRect = newCharacterUi.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, currentY - 0.1f);
                uiRect.anchorMax = new Vector2(1, currentY);
                currentY -= 0.1f;
                newCharacterUi.transform.localScale = new Vector2(1, 1);
                Button b = c.CharacterUi.SimpleView.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectCharacter(b); });
            }
            for (int i = 0; i < _characters.Count; ++i)
            {
                GameObject currentButton = _characters[i].CharacterUi.SimpleView;

                if (i == 0)
                {
                    Helper.SetNavigation(currentButton, inventoryObject,
                        Helper.NavigationDirections.Up);

                    Helper.SetNavigation(inventoryObject, currentButton,
                        Helper.NavigationDirections.Down);
                }
                else if (i > 0)
                {
                    GameObject previousButton = _characters[i - 1].CharacterUi.SimpleView;
                    Helper.SetNavigation(currentButton, previousButton, Helper.NavigationDirections.Up);
                    Helper.SetNavigation(previousButton, currentButton, Helper.NavigationDirections.Down);
                }
            }
        }

        private static DesolationCharacter FindCharacterFromGameObject(GameObject g)
        {
            return _characters.FirstOrDefault(c => c.CharacterUi.GameObject == g);
        }

        public static void RemoveCharacter(DesolationCharacter c, bool isDriver)
        {
            _characters.Remove(c);
            PopulateCharacterUi();
            if (isDriver)
            {
                MenuStateMachine.Instance().NavigateToState("Game Over Menu");
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
            foreach (DesolationCharacter c in _characters)
            {
                if (foundCharacter)
                {
                    RectTransform rect = c.CharacterUi.GameObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (c.CharacterUi.GameObject == g)
                {
                    foundCharacter = true;
                    CharacterUI foundUi = c.CharacterUi;
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
            DesolationCharacter current = FindCharacterFromGameObject(SelectedCharacter.transform.parent.gameObject);
            current.Eat();
        }

        public void CharacterDrink()
        {
            DesolationCharacter current = FindCharacterFromGameObject(SelectedCharacter.transform.parent.gameObject);
            current.Drink();
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = doc.SelectSingleNode("CharacterManager");
            XmlNodeList characterNodes = characterManagerNode.SelectNodes("Character");
            foreach (XmlNode characterNode in characterNodes)
            {
                DesolationCharacter c = new DesolationCharacter();
//                c.Load(characterNode, saveType);
//                _characters.Add(c);
            }
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = SaveController.CreateNodeAndAppend("CharacterManager", doc);
            foreach (DesolationCharacter c in _characters)
            {
                XmlNode characterNode = SaveController.CreateNodeAndAppend("Character", characterManagerNode);
                c.Save(characterNode, saveType);
            }
        }
    }
}