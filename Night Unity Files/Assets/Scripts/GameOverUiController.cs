using DG.Tweening;
using Game.Global;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

public class GameOverUiController : MonoBehaviour
{
    public void Start()
    {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        text.color = UiAppearanceController.InvisibleColour;
        Sequence seq = DOTween.Sequence();
        seq.Append(text.DOColor(Color.white, 2f));
        seq.Append(text.DOColor(UiAppearanceController.InvisibleColour, 5f));
        seq.AppendCallback(() => SceneChanger.ChangeScene("Menu"));
    }
}