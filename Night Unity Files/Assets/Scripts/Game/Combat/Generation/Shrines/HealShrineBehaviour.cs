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
        ParticleSystem p = Helper.FindChildWithName<ParticleSystem>(gameObject, "Heal Indicator");
        p.Emit(100);
        PlayerCombat.Instance.HealthController.Heal(100);
        Helper.FindChildWithName<ColourPulse>(gameObject, "Health Shrine Glow").PulseAndEnd();
        Destroy(this);
    }

    public static void CreateObject(Vector2 position)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Health Shrine");
        GameObject healShrine = Instantiate(_prefab);
        healShrine.transform.position = position;
    }
}