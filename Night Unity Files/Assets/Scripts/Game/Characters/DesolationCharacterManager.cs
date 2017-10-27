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
    public class CharacterManager : DesolationInventory, IPersistenceTemplate, IInputListener
    {
        private static List<Player> _characters = new List<Player>();
        public static Character SelectedCharacter;

        public CharacterManager() : base("Vehicle")
        {
            InputHandler.RegisterInputListener(this);
        }
        
        public void Start()
        {
            TraitLoader.LoadTraits();
            SaveController.AddPersistenceListener(this);
            if (_characters.Count == 0)
            {
                CharacterGenerator.LoadInitialParty();
            }
            foreach (Player playerCharacter in _characters)
            {
                GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", GameObject.Find("Character Section").transform.Find("Content").transform);
                playerCharacter.SetGameObject(characterObject);
            }
            PopulateCharacterUi();
        }
        
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis == InputAxis.Cancel && !isHeld)
            {
                ExitCharacter();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public override void AddItem(MyGameObject g)
        {
            base.AddItem(g);
            Player item = g as Player;
            if (item != null)
            {
                _characters.Add(item);
            }
        }

        public static List<Player> Characters()
        {
            return _characters;
        }

        private static void PopulateCharacterUi()
        {
            GameObject inventoryObject = WorldState.GetInventoryButton();

            foreach (Player playerCharacter in _characters)
            {
                Button b = playerCharacter.CharacterView.SimpleView.GetComponent<Button>();
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
            Player playerCharacter = item as Player;
            if (playerCharacter == null) return item;
            _characters.Remove(playerCharacter);
            PopulateCharacterUi();
            if (playerCharacter.Name == "Driver")
            {
                MenuStateMachine.States.NavigateToState("Game Over Menu");
            }
            return item;
        }

        private static void ChangeCharacterPanel(GameObject g, bool expand)
        {
            foreach (Player playerCharacter in _characters)
            {
                if (playerCharacter.CharacterView.GameObject != g) continue;
                CharacterView foundView = playerCharacter.CharacterView;
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
//                Character c = new Character();
//                c.Load(characterNode, saveType);
//                _characters.Add(c);
            }
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = SaveController.CreateNodeAndAppend("CharacterManager", doc);
            foreach (Character c in _characters)
            {
                XmlNode characterNode = SaveController.CreateNodeAndAppend("Character", characterManagerNode);
                c.Save(characterNode, saveType);
            }
        }
    }
}