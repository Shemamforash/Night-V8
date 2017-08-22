using System.Collections;
using System.Collections.Generic;
using SamsHelper;
using UnityEngine;
using World;

namespace Characters
{
    public class ClassCharacter
    {
        private static Dictionary<string, ClassCharacter> classDictionary = new Dictionary<string, ClassCharacter>();

        private string classTrait, className;

        public static void LoadCharacterClasses()
        {
            List<string> lines = Helper.ReadLinesFromFile("classes");
            for (int i = 0; i < lines.Count; i += 2)
            {
                ClassCharacter newClass = new ClassCharacter(lines[i], lines[i + 1]);
                classDictionary[lines[i]] = newClass;
            }
        }

        public static ClassCharacter FindClass(string name){
            return classDictionary[name];
        }

        public ClassCharacter(string className, string classTrait)
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