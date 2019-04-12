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
        private static readonly Dictionary<CharacterClass, List<JournalEntry>> CharacterStories = new Dictionary<CharacterClass, List<JournalEntry>>();
        private static readonly Dictionary<EnvironmentType, List<JournalEntry>> WandererStories = new Dictionary<EnvironmentType, List<JournalEntry>>();
        private static readonly Dictionary<EnvironmentType, List<JournalEntry>> NecromancerStories = new Dictionary<EnvironmentType, List<JournalEntry>>();
        private static readonly Dictionary<EnvironmentType, List<JournalEntry>> LoreStories = new Dictionary<EnvironmentType, List<JournalEntry>>();

        private static readonly List<JournalEntry> AllEntries = new List<JournalEntry>();
        private static readonly List<JournalEntry> UnlockedEntries = new List<JournalEntry>();
        private static bool _loaded;

        private readonly int _partNumber;
        public readonly string Title;
        public readonly string Text;
        private int _groupNumber;
        private bool _locked = true;


        private static string ToRoman(int number)
        {
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            return "";
        }

        private JournalEntry(string title, string text, int partNumber, int groupNumber, bool titleNeedsConversion = true)
        {
            if (titleNeedsConversion) title = ConvertTitle(title, partNumber);
            Title = title;
            Text = FormatString(text);
            _partNumber = partNumber;
            _groupNumber = groupNumber;
            AllEntries.Add(this);
        }

        private string ConvertTitle(string title, int partNumber)
        {
            return title + " " + ToRoman(partNumber);
        }

        public static void Reset()
        {
            _loaded = false;
            AllEntries.Clear();
            UnlockedEntries.Clear();
            CharacterStories.Clear();
            WandererStories.Clear();
            NecromancerStories.Clear();
            LoreStories.Clear();
            ReadJournals();
        }

        public void Unlock()
        {
            ReadJournals();
            _locked = false;
            UnlockedEntries.Add(this);
        }

        public static void Save(XmlNode root)
        {
            string journalString = "";
            for (int i = 0; i < AllEntries.Count; i++)
            {
                JournalEntry entry = AllEntries[i];
                journalString += entry.Title + ",";
                journalString += entry._locked;
                if (i < AllEntries.Count - 1) journalString += ",";
            }

            root.CreateChild("Journals", journalString);
            root.CreateChild("StorySeen", StoryController.StorySeen);
        }

        public static void Load(XmlNode root)
        {
            ReadJournals();
            string journalString = root.StringFromNode("Journals");
            string[] journalArr = journalString.Split(',');
            for (int i = 0; i < journalArr.Length; i += 2)
            {
                string title = journalArr[i];
                bool locked = bool.Parse(journalArr[i + 1]);
                if (locked) continue;
                JournalEntry entry = AllEntries.Find(j => j.Title == title);
                entry.Unlock();
            }

            StoryController.StorySeen = root.BoolFromNode("StorySeen");
        }

        public static List<JournalEntry> GetCorypthosLore()
        {
            ReadJournals();
            return LoreStories[EnvironmentType.End];
        }

        public static JournalEntry GetStoryEntry()
        {
            ReadJournals();
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType;
            return WandererStories[currentEnvironment].First(j => j._locked);
        }

        public static JournalEntry GetLoreEntry()
        {
            ReadJournals();
            List<JournalEntry> characterEntries = new List<JournalEntry>();

            Player alternateCharacter = CharacterManager.AlternateCharacter;
            if (alternateCharacter != null)
            {
                CharacterClass characterClass = alternateCharacter.CharacterTemplate.CharacterClass;
                if (CharacterStories.ContainsKey(characterClass))
                {
                    characterEntries.AddRange(CharacterStories[characterClass]);
                }
            }

            List<JournalEntry> validEntries = new List<JournalEntry>();
            validEntries.AddRange(characterEntries.FindAll(j => j._locked));
            if (validEntries.Count == 0)
            {
                List<JournalEntry> entries = LoreStories[EnvironmentManager.CurrentEnvironmentType];
                validEntries.AddRange(entries.FindAll(j => j._locked));
            }

            if (validEntries.Count == 0) return null;
            validEntries.Shuffle();
            validEntries.Sort((a, b) => a._partNumber.CompareTo(b._partNumber));
            return validEntries[0];
        }

        public static List<JournalEntry> GetUnlockedEntries()
        {
            ReadJournals();
            UnlockedEntries.Sort((a, b) =>
            {
                int ret = a._groupNumber.CompareTo(b._groupNumber);
                if (ret == 0) ret = a._partNumber.CompareTo(b._partNumber);
                return ret;
            });
            return UnlockedEntries;
        }

        private static void ReadJournals()
        {
            if (_loaded) return;
            LoadCharacterEntries();
            LoadWandererEntries();
            LoadNecromancerEntries();
            LoadLoreEntries();
            _loaded = true;
        }

        private static void LoadCharacterEntries()
        {
            XmlNode root = Helper.OpenRootNode("Characters", "Characters");
            foreach (XmlNode characterNode in root.GetNodesWithName("StoryPart"))
            {
                string title = characterNode.StringFromNode("Title");
                string characterString = characterNode.StringFromNode("Character");
                CharacterClass characterClass = CharacterTemplate.StringToClass(characterString);
                int partNumber = characterNode.IntFromNode("PartNumber");
                string text = characterNode.StringFromNode("Text");
                JournalEntry entry = new JournalEntry(title, text, partNumber, (int) characterClass);
                if (!CharacterStories.ContainsKey(characterClass)) CharacterStories.Add(characterClass, new List<JournalEntry>());
                CharacterStories[characterClass].Add(entry);
                CharacterStories[characterClass].Sort((a, b) => a._partNumber.CompareTo(b._partNumber));
            }
        }

        private static void LoadWandererEntries()
        {
            XmlNode root = Helper.OpenRootNode("Wanderer", "Wanderer");
            foreach (XmlNode wandererNode in root.GetNodesWithName("StoryPart"))
            {
                string title = wandererNode.StringFromNode("Title");
                string environmentString = wandererNode.StringFromNode("Environment");
                EnvironmentType environmentType = Environment.StringToEnvironmentType(environmentString);
                int partNumber = wandererNode.IntFromNode("PartNumber");
                string text = wandererNode.StringFromNode("Text");
                JournalEntry entry = new JournalEntry(title, text, partNumber, (int) environmentType);
                if (!WandererStories.ContainsKey(environmentType)) WandererStories.Add(environmentType, new List<JournalEntry>());
                WandererStories[environmentType].Add(entry);
                WandererStories[environmentType].Sort((a, b) => a._partNumber.CompareTo(b._partNumber));
            }
        }

        private static void LoadNecromancerEntries()
        {
            XmlNode root = Helper.OpenRootNode("Necromancer", "Necromancer");
            foreach (XmlNode necromancerNode in root.GetNodesWithName("StoryPart"))
            {
                string title = necromancerNode.StringFromNode("Title");
                EnvironmentType environmentType = Environment.StringToEnvironmentType(title);
                int partNumber = necromancerNode.IntFromNode("PartNumber");
                string text = necromancerNode.StringFromNode("Text");
                JournalEntry entry = new JournalEntry(title, text, partNumber, (int) environmentType);
                if (!NecromancerStories.ContainsKey(environmentType)) NecromancerStories.Add(environmentType, new List<JournalEntry>());
                NecromancerStories[environmentType].Add(entry);
                NecromancerStories[environmentType].Sort((a, b) => a._partNumber.CompareTo(b._partNumber));
            }
        }

        private static void LoadLoreEntries()
        {
            XmlNode root = Helper.OpenRootNode("Lore", "Lore");
            foreach (XmlNode loreNode in root.GetNodesWithName("StoryPart"))
            {
                string title = loreNode.StringFromNode("Title");
                string environmentString = loreNode.StringFromNode("Environment");
                EnvironmentType environmentType = Environment.StringToEnvironmentType(environmentString);
                bool unique = loreNode.BoolFromNode("Unique");
                string text = loreNode.StringFromNode("Text");
                if (!unique) title = title + " " + ToRoman((int) environmentType + 1);
                JournalEntry entry = new JournalEntry(title, text, 1, (int) environmentType, false);
                if (!LoreStories.ContainsKey(environmentType)) LoreStories.Add(environmentType, new List<JournalEntry>());
                LoreStories[environmentType].Add(entry);
            }
        }

        private static string FormatString(string inputString)
        {
            inputString = inputString.Replace("\n", "");
            Regex compiledRegex = new Regex(@"\s+", RegexOptions.Compiled);
            inputString = compiledRegex.Replace(inputString, " ");
            return inputString.Replace("|br|", "\n");
        }

        public static List<JournalEntry> GetStoryText()
        {
            ReadJournals();
            EnvironmentType currentLevel = EnvironmentManager.CurrentEnvironmentType;
            return NecromancerStories[currentLevel];
        }

        public static int GetStoryCount()
        {
            ReadJournals();
            return WandererStories[EnvironmentManager.CurrentEnvironmentType].Count;
        }
    }
}