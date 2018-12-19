using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Exploration.Environment;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global
{
    public class JournalEntry
    {
        public readonly string Title;
        public readonly string Contents;
        private readonly int _journalGroup;
        private readonly int _numberInGroup;
        private CharacterClass _characterClass = CharacterClass.None;
        private static readonly Dictionary<CharacterClass, List<JournalEntry>> LockedCharacterEntries = new Dictionary<CharacterClass, List<JournalEntry>>();
        private static readonly Dictionary<int, List<JournalEntry>> LockedWandererEntries = new Dictionary<int, List<JournalEntry>>();
        private static readonly Dictionary<int, List<JournalEntry>> LockedEntries = new Dictionary<int, List<JournalEntry>>();
        private static readonly Dictionary<int, string> _mainStoryText = new Dictionary<int, string>();
        private static readonly List<JournalEntry> UnlockedEntries = new List<JournalEntry>();
        private static bool _loaded;
        private readonly bool _isWanderer;


        private JournalEntry(XmlNode journalNode)
        {
            Title = journalNode.StringFromNode("Title");
            Contents = FormatString(journalNode.StringFromNode("Text"));
            _journalGroup = journalNode.IntFromNode("Group");
            _numberInGroup = journalNode.IntFromNode("Part");
            _isWanderer = journalNode.BoolFromNode("IsWanderer");
        }

        public void Unlock()
        {
            ReadJournals();
            UnlockedEntries.Add(this);

            if (_isWanderer)
            {
                if (!LockedWandererEntries.ContainsKey(_journalGroup)) Debug.Log(_journalGroup + " " + _numberInGroup + " " + Title);
                LockedWandererEntries[_journalGroup].Remove(this);
                if (LockedWandererEntries[_journalGroup].Count != 0) return;
                LockedWandererEntries.Remove(_journalGroup);
                return;
            }

            if (_characterClass == CharacterClass.None)
            {
                LockedEntries[_journalGroup].Remove(this);
                if (LockedEntries[_journalGroup].Count != 0) return;
                LockedEntries.Remove(_journalGroup);
                return;
            }

            LockedCharacterEntries[_characterClass].Remove(this);
            if (LockedCharacterEntries[_characterClass].Count != 0) return;
            LockedCharacterEntries.Remove(_characterClass);
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Journals");
            foreach (JournalEntry entry in UnlockedEntries)
            {
                XmlNode entryNode = root.CreateChild("Entry");
                entryNode.CreateChild("Group", entry._journalGroup);
                entryNode.CreateChild("Number", entry._numberInGroup);
                entryNode.CreateChild("CharacterClass", (int) entry._characterClass);
                entryNode.CreateChild("IsWanderer", entry._isWanderer);
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
                int characterClassIndex = entryNode.IntFromNode("CharacterClass");
                bool isWanderer = entryNode.BoolFromNode("IsWanderer");

                if (isWanderer)
                {
                    LockedWandererEntries[group][number].Unlock();
                    continue;
                }

                if (characterClassIndex == 0)
                {
                    LockedEntries[group][number].Unlock();
                    continue;
                }

                LockedCharacterEntries[(CharacterClass) characterClassIndex][number].Unlock();
            }
        }

        public static JournalEntry GetEntry(int part)
        {
            return LockedEntries[part][0];
        }

        public static JournalEntry GetEntry()
        {
            ReadJournals();
            Player player = CharacterManager.SelectedCharacter;
            if (player.CanShowJournal())
            {
                List<JournalEntry> journalGroup;
                if (player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
                    journalGroup = LockedWandererEntries[(int) EnvironmentManager.CurrentEnvironmentType()];
                else
                    journalGroup = LockedCharacterEntries[player.CharacterTemplate.CharacterClass];

                if (journalGroup.Count > 0) return journalGroup[0];
            }

            int[] keys = LockedEntries.Keys.ToArray();
            if (keys.Length == 0) return null;
            List<JournalEntry> randomGroup = LockedEntries[keys.RandomElement()];
            return randomGroup[0];
        }

        public static List<JournalEntry> GetUnlockedEntries()
        {
            ReadJournals();
            UnlockedEntries.Sort((a, b) =>
            {
                int ret = a._journalGroup.CompareTo(b._journalGroup);
                if (ret == 0) ret = a._numberInGroup.CompareTo(b._numberInGroup);
                return ret;
            });
            return UnlockedEntries;
        }

        private static void ReadJournals()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Story", "Story");
            foreach (XmlNode journalNode in Helper.GetNodesWithName(root, "JournalEntry"))
            {
                JournalEntry entry = new JournalEntry(journalNode);
                if (!LockedEntries.ContainsKey(entry._journalGroup)) LockedEntries.Add(entry._journalGroup, new List<JournalEntry>());
                LockedEntries[entry._journalGroup].Add(entry);
                LockedEntries[entry._journalGroup].Sort((a, b) => a._numberInGroup.CompareTo(b._numberInGroup));
            }

            foreach (XmlNode storyNode in Helper.GetNodesWithName(root, "StoryPart"))
            {
                int partNo = storyNode.IntFromNode("Number");
                string storyString = FormatString(storyNode.StringFromNode("Text"));
                _mainStoryText.Add(partNo, storyString);
            }

            foreach (XmlNode characterNode in Helper.GetNodesWithName(root, "CharacterPart"))
            {
                JournalEntry entry = new JournalEntry(characterNode);
                string characterName = entry.Title;
                string[] words = characterName.Split(' ');
                characterName = words[1];
                CharacterClass characterClass = CharacterTemplate.StringToClass(characterName);
                entry._characterClass = characterClass;
                if (!LockedCharacterEntries.ContainsKey(characterClass)) LockedCharacterEntries.Add(characterClass, new List<JournalEntry>());
                LockedCharacterEntries[characterClass].Add(entry);
                LockedCharacterEntries[characterClass].Sort((a, b) => a._numberInGroup.CompareTo(b._numberInGroup));
            }

            foreach (XmlNode wandererNode in Helper.GetNodesWithName(root, "WandererPart"))
            {
                JournalEntry entry = new JournalEntry(wandererNode);
                if (!LockedWandererEntries.ContainsKey(entry._journalGroup)) LockedWandererEntries.Add(entry._journalGroup, new List<JournalEntry>());
                LockedWandererEntries[entry._journalGroup].Add(entry);
                LockedWandererEntries[entry._journalGroup].Sort((a, b) => a._numberInGroup.CompareTo(b._numberInGroup));
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

        public static string GetStoryText()
        {
            int currentLevel = (int) EnvironmentManager.CurrentEnvironmentType();
            ReadJournals();
            return !_mainStoryText.ContainsKey(currentLevel) ? "" : _mainStoryText[currentLevel];
        }
    }
}