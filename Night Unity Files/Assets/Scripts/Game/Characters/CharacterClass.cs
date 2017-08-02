using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

namespace Characters
{
    public class CharacterClass
    {
        private static Dictionary<string, CharacterClass> classDictionary = new Dictionary<string, CharacterClass>();

        private string classTrait, className;

        public static void LoadCharacterClasses()
        {
            string[] lines = Helper.ReadLinesFromFile("classes");
            for (int i = 0; i < lines.Length; i += 2)
            {
                CharacterClass newClass = new CharacterClass(lines[i], lines[i + 1]);
                classDictionary[lines[i]] = newClass;
            }
        }

        public static CharacterClass FindClass(string name){
            return classDictionary[name];
        }

        public CharacterClass(string className, string classTrait)
        {
            this.classTrait = classTrait;
            this.className = className;
        }

        public string ClassTrait()
        {
            return classTrait;
        }

        public string ClassName()
        {
            return className;
        }
    }
}