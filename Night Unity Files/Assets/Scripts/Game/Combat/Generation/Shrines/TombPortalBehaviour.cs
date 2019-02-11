using Facilitating.Audio;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.Libraries;

public class TombPortalBehaviour : CanTakeDamage
{
    protected override void Awake()
    {
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Shadow 5");
        _bloodSpatter = GetComponent<BloodSpatter>();
        CombatManager.AddEnemy(this);
        gameObject.layer = 24;
        HealthController.SetInitialHealth(WorldState.ScaleValue(400), this);
    }

    public override void Kill()
    {
        CreateBoss();
        base.Kill();
    }

    public override string GetDisplayName() => "Sealed Tomb";

    private void CreateBoss()
    {
        ThunderController.Strike(true);
        switch (EnvironmentManager.CurrentEnvironmentType())
        {
            case EnvironmentType.Desert:
                SerpentBehaviour.Create();
                break;
            case EnvironmentType.Mountains:
                StarfishBehaviour.Create();
                break;
            case EnvironmentType.Sea:
                SwarmBehaviour.Create();
                break;
            case EnvironmentType.Ruins:
                OvaBehaviour.Create();
                break;
        }
    }
}