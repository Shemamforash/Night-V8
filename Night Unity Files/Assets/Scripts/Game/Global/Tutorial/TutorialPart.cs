using System.Xml;
using Extensions;
using Facilitating.Persistence;

namespace Game.Global.Tutorial
{
	public class TutorialPart
	{
		public readonly int          SectionNumber, PartNumber;
		public readonly string       Title,         Content, SectionName;
		private         bool         _completed;
		private         TutorialPart _nextPart;

		public TutorialPart(XmlNode node)
		{
			Title         = node.ParseString("Title");
			SectionName   = node.ParseString("SectionName");
			Content       = node.ParseString("Text");
			Content       = Content.Replace("\n", "");
			Content       = Content.Replace(". ", ".\n");
			PartNumber    = node.ParseInt("PartNumber");
			SectionNumber = node.ParseInt("SectionNumber");
			_completed    = node.ParseBool("AutoUnlock");
		}

		public void SaveTutorialPart(XmlNode root)
		{
			root = root.CreateChild("Part");
			root.CreateChild("PartNumber", PartNumber);
			root.CreateChild("Completed",  _completed);
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