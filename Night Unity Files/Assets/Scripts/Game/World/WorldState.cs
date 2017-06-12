using UnityEngine;

namespace World
{
    public class WorldState : MonoBehaviour
    {
		public static int currentDanger = 0;
		public static int daysSpentHere = 0;
		private TimeListener timeListener = new TimeListener();

		private void IncrementDaysSpentHere(){
			++daysSpentHere;
			++currentDanger;
			if(daysSpentHere == 7){
				//TODO gameover
			}
		}

		private void ResetDaysSpentHere(){
			daysSpentHere = 0;
			currentDanger -= 2;
			if(currentDanger < 0){
				currentDanger = 0;
			}
		}

		public WorldState(){
			timeListener.OnDay(IncrementDaysSpentHere);
			timeListener.OnTravel(ResetDaysSpentHere);
		}
    }
}
