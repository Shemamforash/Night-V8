using System.Collections;
using DG.Tweening.Core.Easing;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class ShrineBehaviour : MonoBehaviour
{
    private const float DistanceToTrigger = 0.2f;
    private bool _triggered;
    private ParticleSystem _essence, _void, _burst, _ring;
    private SpriteRenderer _flash;

    public void Awake()
    {
        _essence = Helper.FindChildWithName<ParticleSystem>(gameObject, "Essence Cloud");
        _void = Helper.FindChildWithName<ParticleSystem>(gameObject, "Void");
        _burst = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burst");
        _ring = Helper.FindChildWithName<ParticleSystem>(gameObject, "Ring");
        _flash = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Flash");
        _flash.color = UiAppearanceController.InvisibleColour;
    }
    
    public void Update()
    {
        if (_triggered) return;
        float distanceToPlayer = Vector2.Distance(transform.position, CombatManager.Player().transform.position);
        if (distanceToPlayer > DistanceToTrigger) return;
        _triggered = true;
        StartCoroutine(StartShrine());
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        float time = 0.2f;
        _burst.Emit(50);
        Destroy(_essence.gameObject);
        Destroy(_void.gameObject);
        Destroy(_ring.gameObject);
        _flash.color = Color.white;
        while (time > 0f)
        {
            float lerpVal = 1 -(time / 0.2f);
            _flash.color = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, lerpVal);
            time -= Time.deltaTime;
            yield return null;
        }
        _flash.color = UiAppearanceController.InvisibleColour;
    }

    private IEnumerator StartShrine()
    {
        float radius = 3f;
        float spawnTimeDelay = 0.25f;
        int spawnCount = 20;
        float angleInterval = 360f / spawnCount;
        float currentAngle = Random.Range(0, 360);
        for (int i = 0; i < spawnCount; ++i)
        {
            float currentTime = 0f;
            while (currentTime < spawnTimeDelay)
            {
                EnemyBehaviour ghoul = CombatManager.QueueEnemyToAdd(EnemyType.Ghoul);
                float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius + transform.position.x;
                float y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius + transform.position.y;
                Vector2 ghoulPos = new Vector2(x, y);
                ghoul.transform.position = ghoulPos;
                TeleportInOnly.TeleportObjectIn(ghoul.gameObject);
                currentTime += spawnTimeDelay;
                yield return null;
            }

            currentAngle += angleInterval;
            if (currentAngle > 360f) currentAngle -= 360f;
        }

        Destroy(this);
    }
}