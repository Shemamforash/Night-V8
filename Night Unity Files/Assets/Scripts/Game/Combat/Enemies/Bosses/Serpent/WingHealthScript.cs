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
        ArmourController.SetArmour(Armour.Create(ItemQuality.Shining, Armour.ArmourType.Chest));
        ArmourController.SetArmour(Armour.Create(ItemQuality.Shining, Armour.ArmourType.Head));
    }

    protected override int GetInitialHealth()
    {
        return WorldState.Difficulty() * 2 + 100;
    }

    public void Start()
    {
        SetBoss(SerpentBehaviour.Instance());
    }

    public override string GetDisplayName()
    {
        return "Eo, The Great Despair";
    }

    public override void Kill()
    {
        base.Kill();
        CombatManager.SpawnEnemy(EnemyType.Ghoul, transform.position);
        if (Helper.RollDie(0, 2)) CombatManager.SpawnEnemy(EnemyType.Ghoul, transform.position);
    }
}