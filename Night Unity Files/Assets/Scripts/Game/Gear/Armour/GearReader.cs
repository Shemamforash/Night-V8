using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Game.Gear.Armour
{
    public static class GearReader
    {
        private static List<Accessory> _accessoryList = new List<Accessory>();

        public static void LoadGear()
        {
            TextAsset gearFile = Resources.Load<TextAsset>("XML/Gear");
            XmlDocument gearXml = new XmlDocument();
            gearXml.LoadXml(gearFile.text);
            XmlNode root = gearXml.SelectSingleNode("GearList");
            foreach (XmlNode accessoryNode in root.SelectNodes("Accessory"))
            {
                ReadAccessory(accessoryNode);
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

        public static Accessory GenerateAccessory()
        {
            return _accessoryList[Random.Range(0, _accessoryList.Count)];
        }
    }
}