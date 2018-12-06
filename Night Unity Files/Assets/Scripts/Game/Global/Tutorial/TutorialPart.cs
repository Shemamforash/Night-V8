using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global.Tutorial
{
    public class TutorialPart
    {
        public readonly Vector2 MinOffset, MaxOffset;
        public readonly string Title, Content;
        public readonly int SectionNumber, PartNumber;
        private bool _completed;
        private TutorialPart _nextPart;

        public TutorialPart(XmlNode node)
        {
            MinOffset = node.StringFromNode("MinOffset").ToVector2();
            MaxOffset = node.StringFromNode("MaxOffset").ToVector2();
            Title = node.StringFromNode("Title");
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
    }
    
    
    
    
}