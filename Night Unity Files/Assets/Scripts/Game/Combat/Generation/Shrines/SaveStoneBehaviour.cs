using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Combat.Ui;
using SamsHelper.Input;
using UnityEngine;

public class SaveStoneBehaviour : MonoBehaviour, IInputListener
{
    private bool _saved;
    private static GameObject _saveStonePrefab;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_saved) return;
        InputHandler.RegisterInputListener(this);
        PlayerUi.SetEventText("There is space to carve your journey so far [T]");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_saved) return;
        InputHandler.UnregisterInputListener(this);
        PlayerUi.FadeTextOut();
    }

    public static void Generate(Vector2 position)
    {
        if (_saveStonePrefab == null) _saveStonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Save Stone");
        GameObject saveStoneObject = Instantiate(_saveStonePrefab);
        saveStoneObject.transform.position = position;
        PathingGrid.AddBlockingArea(position, 0.5f);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.TakeItem) return;
        SaveController.SaveGame();
        foreach (ParticleSystem componentsInChild in transform.GetComponentsInChildren<ParticleSystem>(transform))
        {
            componentsInChild.Stop();
        }

        foreach (TrailRenderer componentsInChild in transform.GetComponentsInChildren<TrailRenderer>(transform))
        {
            componentsInChild.DOTime(0, 1);
        }
        _saved = true;
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}
