using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;
using Persistence;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        private List<Character> characters = new List<Character>();
        private TimeListener timeListener = new TimeListener();

		public void Awake(){
			Traits.LoadTraits();
			if(Settings.party != null){
				characters = Settings.party;
			} else {
				characters = CharacterGenerator.LoadInitialParty();
			}
		}
    }

}
