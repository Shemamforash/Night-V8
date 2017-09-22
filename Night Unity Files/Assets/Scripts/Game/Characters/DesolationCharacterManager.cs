using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Characters;
using Facilitating.MenuNavigation;
using Facilitating.Persistence;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class DesolationCharacterManager : DesolationInventory , IPersistenceTemplate
    {
        private static List<DesolationCharacter> _characters = new List<DesolationCharacter>();
        public static DesolationCharacter SelectedCharacter;

        public DesolationCharacterManager() : base("Vehicle")
        {
            TraitLoader.LoadTraits();
            SaveController.AddPersistenceListener(this);
        }
        
        public void Start()
        {
            InputSpeaker.Instance().AddOnPressEvent(InputAxis.Cancel, ExitCharacter);
            if (_characters.Count == 0)
            {
                DesolationCharacterGenerator.LoadInitialParty();
            }
            PopulateCharacterUi();
        }

        public override void AddItem(MyGameObject g)
        {
            base.AddItem(g);
            DesolationCharacter item = g as DesolationCharacter;
            if (item != null)
            {
                _characters.Add(item);
            }
        }
        
        public static List<DesolationCharacter> Characters()
        {
            return _characters;
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
                        Direction.Up);

                    Helper.SetNavigation(inventoryObject, currentButton,
                        Direction.Down);
                }
                else if (i > 0)
                {
                    GameObject previousButton = _characters[i - 1].CharacterUi.SimpleView;
                    Helper.SetNavigation(currentButton, previousButton, Direction.Up);
                    Helper.SetNavigation(previousButton, currentButton, Direction.Down);
                }
            }
        }

        private static DesolationCharacter FindCharacterFromGameObject(GameObject g)
        {
            return _characters.FirstOrDefault(c => c.CharacterUi.GameObject == g);
        }

        public override MyGameObject RemoveItem(DesolationCharacter c)
        {
            base.RemoveItem(c);
            _characters.Remove(c);
            PopulateCharacterUi();
            if (c.Name == "Driver")
            {
                MenuStateMachine.States.NavigateToState("Game Over Menu");
            }
            return c;
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
                SetDetailedViewActive(false, SelectedCharacter.GameObject.transform);
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
            DesolationCharacter current = FindCharacterFromGameObject(SelectedCharacter.GameObject.transform.parent.gameObject);
            current.Attributes.Eat();
        }

        public void CharacterDrink()
        {
            DesolationCharacter current = FindCharacterFromGameObject(SelectedCharacter.GameObject.transform.parent.gameObject);
            current.Attributes.Drink();
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = doc.SelectSingleNode("CharacterManager");
            XmlNodeList characterNodes = characterManagerNode.SelectNodes("Character");
            foreach (XmlNode characterNode in characterNodes)
            {
//                DesolationCharacter c = new DesolationCharacter();
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