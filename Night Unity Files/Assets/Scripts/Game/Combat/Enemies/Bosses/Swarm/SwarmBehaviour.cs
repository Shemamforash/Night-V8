using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using Sirenix.Utilities;
using UnityEngine;

public class SwarmBehaviour : Boss
{
    private static SwarmBehaviour _instance;
    private bool _secondStageStarted;

    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Boss");
        GameObject swarm = Instantiate(prefab);
        swarm.transform.position = Vector2.zero;
        _instance = swarm.GetComponent<SwarmBehaviour>();
    }

    public override void UnregisterSection(BossSectionHealthController segment)
    {
        Sections.Remove(segment);
    }

    public bool CheckChangeToStageTwo()
    {
        if (_secondStageStarted) return false;
        StartCoroutine(StartSecondStage());
        return true;
    }

    private IEnumerator StartSecondStage()
    {
        _secondStageStarted = true;
        SwarmSegmentBehaviour.Active.ForEach(s => s.Collapse(transform.position));
        yield return new WaitForSeconds(2f);
        GameObject heavyPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Heavy Swarm Segment");
        for (int i = 0; i < 6; ++i)
        {
            float angle = 360f / 6 * i;
            Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 2f, transform.position);
            TeleportInOnly.TeleportIn(position);
            GameObject heavySwarmSegment = Instantiate(heavyPrefab, transform, true);
            heavySwarmSegment.transform.position = position;
            heavySwarmSegment.GetComponent<HeavySwarmSegmentBehaviour>().SetAngleOffset(angle);
        }
    }

    public static List<CanTakeDamage> GetAllSegments()
    {
        return new List<CanTakeDamage>(_instance.Sections);
    }

    public static SwarmBehaviour Instance() => _instance;

    public override void Kill()
    {
        base.Kill();
        for (int i = SwarmSegmentBehaviour.Active.Count - 1; i >= 0; --i)
            SwarmSegmentBehaviour.Active[i].Kill();
        for (int i = HeavySwarmSegmentBehaviour.Active.Count - 1; i >= 0; --i)

        {
            HeavySwarmSegmentBehaviour segment = HeavySwarmSegmentBehaviour.Active[i];
            LeafBehaviour.CreateLeaves(segment.transform.position);
            Destroy(segment.gameObject);
        }
    }
}