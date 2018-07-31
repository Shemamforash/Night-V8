using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class EnemyCampfire : IPersistenceTemplate
    {
        public readonly Vector2 FirePosition;

        public EnemyCampfire(Vector2 position)
        {
            FirePosition = position;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode campfireNode = SaveController.CreateNodeAndAppend("Campfire", doc);
            SaveController.CreateNodeAndAppend("Position", campfireNode, FirePosition.ToString());
            return doc;
        }

        public void CreateObject()
        {
            FireGenerator.Create(FirePosition);
            PathingGrid.AddBlockingArea(FirePosition, 0.5f);
        }
    }
}