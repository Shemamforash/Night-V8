using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

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
        CombatManager.SpawnEnemy(Helper.RollDie(0, 3) ? EnemyType.Ghast : EnemyType.Ghoul, transform.position);
        Parent.UnregisterSection(this);
        CombatManager.RemoveEnemy(this);
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<PolygonCollider2D>());
        DamageSpriteFlash damageSpriteFlash = GetComponent<DamageSpriteFlash>();
        damageSpriteFlash.Kill();
        Destroy(damageSpriteFlash);
        Destroy(this);
        GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 0.2f), 1f);
    }
}