using Game.Combat.Misc;

public class SpermSegmentBehaviour : CanTakeDamage {
	private SpermBehaviour _spermParent;

	public override string GetDisplayName()
	{
		return "Sperm";
	}

	public void SetSperm(SpermBehaviour spermBehaviour)
	{
		_spermParent = spermBehaviour;
	}
}
