using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;

public class WingHealthScript : BossSectionHealthController
{
    protected override void Awake()
    {
        base.Awake();
        ArmourController.AutoGenerateArmour();
    }

    protected override int GetInitialHealth()
    {
        return 100;
    }

    public void Start()
    {
        SetBoss(SerpentBehaviour.Instance());
    }

    public override string GetDisplayName()
    {
        return "Eo's Despair";
    }

    public override void Kill()
    {
        base.Kill();
        CombatManager.SpawnEnemy(Helper.RollDie(0, 3) ? EnemyType.Ghast : EnemyType.Ghoul, transform.position);
    }
}