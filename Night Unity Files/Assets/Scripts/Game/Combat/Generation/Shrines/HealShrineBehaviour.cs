using Facilitating.UIControllers;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class HealShrineBehaviour : MonoBehaviour
{
    private static GameObject _prefab;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HealPlayer();
    }

    private void HealPlayer()
    {
        ParticleSystem p = gameObject.FindChildWithName<ParticleSystem>("Heal Indicator");
        p.Emit(100);
        PlayerCombat.Instance.HealthController.Heal(100);
        gameObject.FindChildWithName<ColourPulse>("Health Shrine Glow").PulseAndEnd();
        GetComponent<CompassItem>().Die();
        Destroy(this);
    }

    public static void CreateObject(Vector2 position)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Health Shrine");
        GameObject healShrine = Instantiate(_prefab);
        healShrine.transform.position = position;
        PathingGrid.AddBlockingArea(position, 0.5f);
    }
}