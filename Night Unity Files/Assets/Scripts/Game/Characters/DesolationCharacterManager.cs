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
using SamsHelper.ReactiveUI.Elements;
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
                foreach (Player playerCharacter in CharacterGenerator.LoadInitialParty())
                {
                    AddCharacter(playerCharacter);
                }
            }
            PopulateCharacterUi();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis == InputAxis.CancelCover && !isHeld)
            {
                ExitCharacter();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void AddCharacter(Player playerCharacter)
        {
            Transform characterAreaTransform = GameObject.Find("Character Section").transform.Find("Content").transform;
            if (Items().Count > 0)
            {
                Helper.AddDelineator(characterAreaTransform);
            }
            GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", characterAreaTransform);
            playerCharacter.SetGameObject(characterObject);
            AddItem(playerCharacter);
            _characters.Add(playerCharacter);
        }

        public static List<Player> Characters()
        {
            return _characters;
        }

        private static void PopulateCharacterUi()
        {
            Button inventoryButton = WorldView.GetInventoryButton();

            foreach (Player playerCharacter in _characters)
            {
                Button b = playerCharacter.CharacterView.SimpleView.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectCharacter(b); });
            }
            for (int i = 1; i < _characters.Count; ++i)
            {
                Button previousButton = _characters[i - 1].CharacterView.SimpleView.GetComponent<Button>();
                Helper.SetReciprocalNavigation(previousButton, _characters[i].CharacterView.SimpleView.GetComponent<Button>());
            }
            Helper.SetReciprocalNavigation(inventoryButton, _characters[0].CharacterView.SimpleView.GetComponent<Button>());
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