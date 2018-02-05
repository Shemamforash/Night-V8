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
        private static readonly List<Player.Player> _characters = new List<Player.Player>();
        public static Player.Player SelectedCharacter;

        public CharacterManager() : base("Vehicle")
        {
            InputHandler.RegisterInputListener(this);
        }

        public void Start()
        {
            CharacterTemplateLoader.LoadTemplates();
            SaveController.AddPersistenceListener(this);
            if (_characters.Count == 0)
            {
                foreach (Player.Player playerCharacter in PlayerGenerator.LoadInitialParty())
                {
                    AddCharacter(playerCharacter);
                }
            }
            PopulateCharacterUi();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis == InputAxis.Reload && !isHeld)
            {
//                ExitCharacter();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void AddCharacter(Player.Player playerCharacter)
        {
            Transform characterAreaTransform = GameObject.Find("Character Section").transform.Find("Content").transform;
            if (Items().Count > 0)
            {
                Helper.AddDelineator(characterAreaTransform);
            }
            GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", characterAreaTransform);
            characterObject.name = playerCharacter.Name;
            playerCharacter.SetGameObject(characterObject);
            AddItem(playerCharacter);
            _characters.Add(playerCharacter);
            _characters.ForEach(c => c.CharacterView.RefreshNavigation());
        }

        public static List<Player.Player> Characters()
        {
            return _characters;
        }

        private static void PopulateCharacterUi()
        {
//            Button inventoryButton = WorldView.GetInventoryButton();

//            foreach (Player playerCharacter in _characters)
//            {
//                EnhancedButton b = playerCharacter.CharacterView.SimpleView.GetComponent<EnhancedButton>();
//                b.AddOnSelectEvent(delegate
//                {
//                    ExitCharacter();
//                    SelectCharacter(b.Button());
//                });
//            }
//            for (int i = 1; i < _characters.Count; ++i)
//            {
//                Button previousButton = _characters[i - 1].CharacterView.SimpleView.GetComponent<Button>();
//                Helper.SetReciprocalNavigation(previousButton, _characters[i].CharacterView.SimpleView.GetComponent<Button>());
//            }
//            Helper.SetReciprocalNavigation(inventoryButton, _characters[0].CharacterView.SimpleView.GetComponent<Button>());
        }

        public override MyGameObject RemoveItem(MyGameObject item)
        {
            base.RemoveItem(item);
            Player.Player playerCharacter = item as Player.Player;
            if (playerCharacter == null) return item;
            _characters.Remove(playerCharacter);
            PopulateCharacterUi();
            _characters.ForEach(c => c.CharacterView.RefreshNavigation());
            if (playerCharacter.Name == "Driver")
            {
                MenuStateMachine.ShowMenu("Game Over Menu");
            }
            return item;
        }

        public static void ExitCharacter(Player.Player character)
        {
            character.CharacterView.SwitchToSimpleView();
        }

        public static void SelectCharacter(Player.Player player)
        {
            SelectedCharacter = player;
            player.CharacterView.SwitchToDetailedView();
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

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            doc = base.Save(doc, saveType);
            foreach (Player.Player c in _characters)
            {
//                XmlNode characterNode = SaveController.CreateNodeAndAppend("Character", doc);
//                c.Save(characterNode, saveType);
            }
            return doc;
        }

        public static Player.Player PreviousCharacter(Player.Player character)
        {
            for (int i = 0; i < _characters.Count; ++i)
            {
                if (_characters[i] != character) continue;
                if (i != 0)
                {
                    return _characters[i - 1];
                }
                break;
            }
            return null;
        }

        public static Player.Player NextCharacter(Player.Player character)
        {
            for (int i = 0; i < _characters.Count; ++i)
            {
                if (_characters[i] != character) continue;
                if (i != _characters.Count - 1)
                {
                    return _characters[i + 1];
                }
                break;
            }
            return null;
        }
    }
}