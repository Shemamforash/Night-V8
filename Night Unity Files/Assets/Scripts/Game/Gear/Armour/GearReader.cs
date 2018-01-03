using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Game.Gear.Armour
{
    public static class GearReader
    {
        private static List<Armour> _armourList = new List<Armour>();
        private static List<Accessory> _accessoryList = new List<Accessory>();
        
        public static void LoadGear()
        {
            TextAsset gearFile = Resources.Load<TextAsset>("Gear");
            XmlDocument gearXml = new XmlDocument();
            gearXml.LoadXml(gearFile.text);
            XmlNode root = gearXml.SelectSingleNode("GearList");
            foreach (XmlNode accessoryNode in root.SelectNodes("Accessory"))
            {
                ReadAccessory(accessoryNode);
            }
            foreach (XmlNode armourNode in root.SelectNodes("Armour"))
            {
                ReadArmour(armourNode);
            }
        }

        private static void ReadAccessory(XmlNode node)
        {
            string name = node.SelectSingleNode("Name").InnerText;
            int weight = int.Parse(node.SelectSingleNode("Weight").InnerText);
            string description = node.SelectSingleNode("Description").InnerText;
            string effect = node.SelectSingleNode("Effect").InnerText;
            Accessory a = new Accessory(name, weight, description, effect);
            _accessoryList.Add(a);
        }

        private static void ReadArmour(XmlNode node)
        {
            string name = node.SelectSingleNode("Name").InnerText;
            int weight = int.Parse(node.SelectSingleNode("Weight").InnerText);
            string description = node.SelectSingleNode("Description").InnerText;
            int armour = int.Parse(node.SelectSingleNode("Armour").InnerText);
            int perceptionModifier = int.Parse(node.SelectSingleNode("Perception").InnerText);
            int willpowerModifier = int.Parse(node.SelectSingleNode("Willpower").InnerText);
            int strengthModifier = int.Parse(node.SelectSingleNode("Strength").InnerText);
            int enduranceModifier = int.Parse(node.SelectSingleNode("Endurance").InnerText);
            Armour a = new Armour(name, weight, description, armour, perceptionModifier, willpowerModifier, strengthModifier, enduranceModifier);
            _armourList.Add(a);
        }

        public static Armour GenerateArmour()
        {
            return _armourList[Random.Range(0, _armourList.Count)];
        }

        public static Accessory GenerateAccessory()
        {
            return _accessoryList[Random.Range(0, _accessoryList.Count)];
        }
    }
}