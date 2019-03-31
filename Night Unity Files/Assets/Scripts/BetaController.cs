using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class BetaController : MonoBehaviour
{
    private CloseButtonController _closeButton;

    private void Start()
    {
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetOnClick(SceneChanger.GoToMainMenuScene);
        _closeButton.Enable();
        _closeButton.UseAcceptInput();
        _closeButton.Button().Select();
    }
}