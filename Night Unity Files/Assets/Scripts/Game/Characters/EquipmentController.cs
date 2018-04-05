using System.Collections.Generic;
using System.Xml;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public class EquipmentController : IPersistenceTemplate
    {
        private readonly Dictionary<GearSubtype, GearItem> _equippedGear = new Dictionary<GearSubtype, GearItem>();

        
        public void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new NotImplementedException();
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            
            return doc;
        }
    }
}