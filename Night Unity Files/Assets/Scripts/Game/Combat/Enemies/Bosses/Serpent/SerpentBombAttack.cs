using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBombAttack : MonoBehaviour
{
    private float _timeToNextBomb;
    private float _minTimeToBomb = -1;

    public void SetMinTimeToBomb(float timeToBomb)
    {
        _minTimeToBomb = timeToBomb;
    }

    public void Update()
    {
        if (_minTimeToBomb == -1) return;
        if (!CombatManager.IsCombatActive()) return;
        _timeToNextBomb -= Time.deltaTime;
        if (_timeToNextBomb > 0) return;
        _timeToNextBomb = Random.Range(_minTimeToBomb, _minTimeToBomb * 2f);
        Vector2 randomPosition = AdvancedMaths.RandomVectorWithinRange(transform.position, 7.5f);
        Explosion explosion = Explosion.CreateExplosion(randomPosition, 20);
        SerpentBehaviour.Instance().GetSections().ForEach(s => { explosion.AddIgnoreTarget(s); });
        explosion.Detonate();
    }
}