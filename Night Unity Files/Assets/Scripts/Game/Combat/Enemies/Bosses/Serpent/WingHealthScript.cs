using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;

public class WingHealthScript : BossSectionHealthController
{
    protected override int GetInitialHealth()
    {
        return 10;
    }

    public override void Start()
    {
        SetBoss(SerpentBehaviour.Instance());
        base.Start();
    }

    public override string GetDisplayName()
    {
        return "Serpent";
    }
}