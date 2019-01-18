using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global.Tutorial
{
    public class TutorialPart
    {
        public readonly string Title, Content, SectionName;
        public readonly int SectionNumber, PartNumber;
        private bool _completed;
        private TutorialPart _nextPart;

        public TutorialPart(XmlNode node)
        {
            Title = node.StringFromNode("Title");
            SectionName = node.StringFromNode("SectionName");
            Content = node.StringFromNode("Text");
            Content = Content.Replace("\n", "");
            Content = Content.Replace(". ", ".\n");
            PartNumber = node.IntFromNode("PartNumber");
            SectionNumber = node.IntFromNode("SectionNumber");
        }

        public void SaveTutorialPart(XmlNode root)
        {
            root = root.CreateChild("Part");
            root.CreateChild("PartNumber", PartNumber);
            root.CreateChild("Completed", _completed);
        }

        public void SetNextPart(TutorialPart nextPart)
        {
            _nextPart = nextPart;
        }

        public TutorialPart NextPart() => _nextPart;

        public void MarkComplete() => _completed = true;

        public bool IsComplete() => _completed;

        public bool IsViewable() => _completed || !TutorialManager.Active();
    }
}