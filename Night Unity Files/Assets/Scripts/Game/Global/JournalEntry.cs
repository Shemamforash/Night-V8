using System.Collections.Generic;

namespace Game.Global
{
    public class JournalEntry
    {
        public readonly string Title;
        public readonly string Contents;

        public static List<JournalEntry> JournalEntries = new List<JournalEntry>();

        public JournalEntry(string title, string contents)
        {
            Title = title;
            Contents = contents;
        }
    }
}