using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterManager : DesolationInventory, IPersistenceTemplate
    {
        private static readonly List<Player.Player> _characters = new List<Player.Player>();
        public static Player.Player SelectedCharacter;

        public CharacterManager() : base("Vehicle")
        {
        }

        public void Start()
        {
            PlayerGenerator.LoadTemplates();
            SaveController.AddPersistenceListener(this);
            if (_characters.Count != 0) return;
            foreach (Player.Player playerCharacter in PlayerGenerator.LoadInitialParty())
            {
                AddCharacter(playerCharacter);
            }
        }

        private void AddCharacter(Player.Player playerCharacter)
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

        public List<Weapon> Weapons()
        {
            return weapons;
        }

        public List<ArmourPlate> Armour()
        {
            return armour;
        }

        public List<Accessory> Accessories()
        {
            return accessories;
        }

        private readonly List<Weapon> weapons = new List<Weapon>();
        private readonly List<ArmourPlate> armour = new List<ArmourPlate>();
        private readonly List<Accessory> accessories = new List<Accessory>();

        protected override void AddItem(MyGameObject item)
        {
            base.AddItem(item);
            if(item is Weapon) weapons.Add((Weapon)item);
            if(item is ArmourPlate) armour.Add((ArmourPlate)item);
            if(item is Accessory) accessories.Add((Accessory)item);
        }
        
        public override MyGameObject RemoveItem(MyGameObject item)
        {
            base.RemoveItem(item);
            Player.Player playerCharacter = item as Player.Player;
            weapons.Remove(item as Weapon);
            armour.Remove(item as ArmourPlate);
            accessories.Remove(item as Accessory);
            if (playerCharacter == null) return item;
            _characters.Remove(playerCharacter);
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