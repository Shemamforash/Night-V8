using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;

public class WingHealthScript : BossSectionHealthController
{
    private List<EnemyTemplate> _allowedEnemies;

    protected override void Awake()
    {
        base.Awake();
        ArmourController.SetArmour(Armour.Create(ItemQuality.Shining, Armour.ArmourType.Chest));
        ArmourController.SetArmour(Armour.Create(ItemQuality.Shining, Armour.ArmourType.Head));
        _allowedEnemies = WorldState.GetAllowedNightmareEnemyTypes();
    }

    protected override int GetInitialHealth()
    {
        return 20;
    }

    public override void Start()
    {
        SetBoss(SerpentBehaviour.Instance());
        base.Start();
    }

    public override string GetDisplayName()
    {
        return "Eo, The Great Despair";
    }

    public override void Kill()
    {
        base.Kill();
        float size = 1;
        while (size > 0)
        {
            _allowedEnemies.Shuffle();
            foreach (EnemyTemplate e in _allowedEnemies)
            {
                if (e.Value > size) continue;
                CombatManager.SpawnEnemy(e.EnemyType, transform.position);
                size -= e.Value;
                break;
            }
        }
    }
}