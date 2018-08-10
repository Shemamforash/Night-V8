using Game.Combat.Enemies.Bosses;

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
}