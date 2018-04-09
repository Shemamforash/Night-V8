using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Persistence;

namespace Facilitating.UI
{
    public class PermadeathToggler : Toggler, IPersistenceTemplate
    {
        private bool _permadeathOn;

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Game) _permadeathOn = SaveController.ParseBoolFromSubNode(doc, nameof(_permadeathOn));
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Game) SaveController.CreateNodeAndAppend(nameof(_permadeathOn), doc);
            return doc;
        }

        public override void Awake()
        {
            base.Awake();
            SaveController.AddPersistenceListener(this);
        }

        protected override void On()
        {
            base.On();
            _permadeathOn = true;
        }

        protected override void Off()
        {
            base.Off();
            _permadeathOn = false;
        }
    }
}