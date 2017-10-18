using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.MenuNavigation;
using Facilitating.Persistence;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class DesolationCharacterManager : DesolationInventory, IPersistenceTemplate
    {
        private static List<DesolationCharacter> _characters = new List<DesolationCharacter>();
        public static DesolationCharacter SelectedCharacter;

        public DesolationCharacterManager() : base("Vehicle")
        {
        }
        
        public void Start()
        {
            TraitLoader.LoadTraits();
            SaveController.AddPersistenceListener(this);
            InputSpeaker.Instance().AddOnPressEvent(InputAxis.Cancel, ExitCharacter);
            if (_characters.Count == 0)
            {
                DesolationCharacterGenerator.LoadInitialParty();
            }
            foreach (DesolationCharacter character in _characters)
            {
                GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", GameObject.Find("Character Section").transform.Find("Content").transform);
                character.SetGameObject(characterObject);
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

            foreach (DesolationCharacter c in _characters)
            {
                Button b = c.CharacterView.SimpleView.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectCharacter(b); });
            }
            for (int i = 1; i < _characters.Count; ++i)
            {
                GameObject previousButton = _characters[i - 1].CharacterView.SimpleView;
                Helper.SetReciprocalNavigation(_characters[i].CharacterView.SimpleView, previousButton);
            }
            Helper.SetReciprocalNavigation(inventoryObject, _characters[0].CharacterView.SimpleView);
        }

        public override MyGameObject RemoveItem(MyGameObject item)
        {
            base.RemoveItem(item);
            DesolationCharacter c = item as DesolationCharacter;
            if (c == null) return item;
            _characters.Remove(c);
            PopulateCharacterUi();
            if (c.Name == "Driver")
            {
                MenuStateMachine.States.NavigateToState("Game Over Menu");
            }
            return item;
        }

        private static void ChangeCharacterPanel(GameObject g, bool expand)
        {
            foreach (DesolationCharacter c in _characters)
            {
                if (c.CharacterView.GameObject != g) continue;
                CharacterView foundView = c.CharacterView;
                if (expand)
                {
                    foundView.SwitchToDetailedView();
                }
                else
                {
                    foundView.SwitchToSimpleView();
                }
            }
        }

        public static void ExitCharacter()
        {
            if (SelectedCharacter == null) return;
            SetDetailedViewActive(false, SelectedCharacter.GetGameObject().transform);
            SelectedCharacter = null;
        }

        private static void SetDetailedViewActive(bool active, Transform characterUiObject)
        {
            ChangeCharacterPanel(characterUiObject.gameObject, active);
        }

        public static void SelectCharacter(Selectable s)
        {
            if (SelectedCharacter != null)
            {
                ExitCharacter();
            }
            SelectedCharacter = _characters.FirstOrDefault(c => c.CharacterView.SimpleView.GetComponent<Button>() == s);
            SetDetailedViewActive(true, s.transform.parent);
        }

        public void SelectActions(Selectable s)
        {
//            _actionSelectable = s;
//            _actionContainer.gameObject.SetActive(true);
//            _actionContainer.GetChild(0).GetComponent<Selectable>().Select();
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
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