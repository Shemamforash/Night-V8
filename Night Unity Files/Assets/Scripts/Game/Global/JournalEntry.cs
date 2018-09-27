using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;

namespace Game.Global
{
    public class JournalEntry
    {
        public readonly string Title;
        public readonly string Contents;
        private readonly int _journalGroup;
        private readonly int _numberInGroup;

        private static readonly Dictionary<int, List<JournalEntry>> LockedEntries = new Dictionary<int, List<JournalEntry>>();
        private static readonly Dictionary<int, string> _mainStoryText = new Dictionary<int, string>();
        private static readonly List<JournalEntry> UnlockedEntries = new List<JournalEntry>();
        private static bool _loaded;

        private JournalEntry(XmlNode journalNode)
        {
            Title = journalNode.StringFromNode("Title");
            Contents = FormatString(journalNode.StringFromNode("Text"));
            _journalGroup = journalNode.IntFromNode("Group");
            _numberInGroup = journalNode.IntFromNode("Part");
            if (!LockedEntries.ContainsKey(_journalGroup)) LockedEntries.Add(_journalGroup, new List<JournalEntry>());
            LockedEntries[_journalGroup].Add(this);
            LockedEntries[_journalGroup].Sort((a, b) => a._numberInGroup.CompareTo(b._numberInGroup));
        }

        public void Unlock()
        {
            ReadJournals();
            UnlockedEntries.Add(this);
            LockedEntries[_journalGroup].Remove(this);
            if (LockedEntries[_journalGroup].Count != 0) return;
            LockedEntries.Remove(_journalGroup);
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Journals");
            foreach (JournalEntry entry in UnlockedEntries)
            {
                XmlNode entryNode = root.CreateChild("Entry");
                entryNode.CreateChild("Group", entry._journalGroup);
                entryNode.CreateChild("Number", entry._numberInGroup);
            }
        }

        public static void Load(XmlNode root)
        {
            ReadJournals();
            XmlNode journalNode = root.SelectSingleNode("Journals");
            foreach (XmlNode entryNode in journalNode.SelectNodes("Entry"))
            {
                int group = entryNode.IntFromNode("Group");
                int number = entryNode.IntFromNode("Number");
                LockedEntries[group][number].Unlock();
            }
        }

        public static JournalEntry GetEntry()
        {
            ReadJournals();
            List<JournalEntry> randomGroup = LockedEntries[LockedEntries.Keys.ToArray().RandomElement()];
            return LockedEntries.Count == 0 ? null : randomGroup[0];
        }

        public static List<JournalEntry> GetUnlockedEntries()
        {
            ReadJournals();
            return UnlockedEntries;
        }

        private static void ReadJournals()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Story", "Story");
            foreach (XmlNode journalNode in Helper.GetNodesWithName(root, "JournalEntry"))
                new JournalEntry(journalNode);
            
            foreach (XmlNode storyNode in Helper.GetNodesWithName(root, "StoryPart"))
            {
                int partNo = storyNode.IntFromNode("Number");
                string storyString = FormatString(storyNode.StringFromNode("Text"));
                _mainStoryText.Add(partNo, storyString);
            }

            _loaded = true;
        }

        private static string FormatString(string inputString)
        {
            inputString = inputString.Replace("\n", "");
            Regex compiledRegex = new Regex(@"\s+", RegexOptions.Compiled);
            inputString = compiledRegex.Replace(inputString, " ");
            return inputString.Replace("[br]", "\n");
        }

        public static string GetStoryText(int currentLevel)
        {
            ReadJournals();
            return !_mainStoryText.ContainsKey(currentLevel) ? "" : _mainStoryText[currentLevel];
        }
    }
}