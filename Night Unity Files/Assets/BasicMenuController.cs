using Extensions;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine.SceneManagement;

public class BasicMenuController : Menu
{
    private CloseButtonController _closeButton;

    protected override void Awake()
    {
        base.Awake();
        if (SceneManager.GetActiveScene().name != "Menu") return;
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Back Button");
    }

    public override void PreEnter()
    {
        base.Enter();
        if (_closeButton == null) return;
        _closeButton.Enable();
    }

    public override void Exit()
    {
        base.Exit();
        if (_closeButton == null) return;
        _closeButton.Disable();
    }
}