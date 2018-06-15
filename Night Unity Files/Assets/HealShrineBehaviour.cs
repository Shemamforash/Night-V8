using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class HealShrineBehaviour : MonoBehaviour
{
    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(PlayerCombat.Instance.transform.position, transform.position);
        if (distanceToPlayer > 0.2f) return;
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
}