using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class LoadingController : Menu
{
    private ParticleSystem _dots, _trail;

    public override void Awake()
    {
        base.Awake();
        _dots = gameObject.FindChildWithName<ParticleSystem>("Dots");
        _trail = gameObject.FindChildWithName<ParticleSystem>("Trail");
    }
    
    public override void Enter()
    {
        base.Enter();
        _dots.Play();
        _trail.Play();
    }
}
