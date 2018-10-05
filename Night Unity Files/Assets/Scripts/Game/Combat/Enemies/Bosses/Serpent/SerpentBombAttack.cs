using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBombAttack : MonoBehaviour
{
    private float _timeToNextBomb;

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        _timeToNextBomb -= Time.deltaTime;
        if (_timeToNextBomb > 0) return;
        _timeToNextBomb = Random.Range(0.5f, 1f);
        Vector2 randomPosition = AdvancedMaths.RandomVectorWithinRange(transform.position, 5f);
        Explosion explosion = Explosion.CreateExplosion(randomPosition, 20);
        SerpentBehaviour.Instance().GetSections().ForEach(s => { explosion.AddIgnoreTarget(s); });
        explosion.Detonate();
    }
}