namespace Game.Combat.Misc
{
	public interface ICombatEvent
	{
		float  InRange();
		string GetEventText();
		void   Activate();
	}
}