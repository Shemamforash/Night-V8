using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using SamsHelper.Input;
using UnityEngine;

public class SaveStoneBehaviour : MonoBehaviour, IInputListener
{
    private static GameObject _saveStonePrefab;
    private Region _region;

    private void OnTriggerEnter2D(Collider2D other)
    {
        InputHandler.RegisterInputListener(this);
        PlayerUi.SetEventText("There is space to carve your journey so far [T]");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        InputHandler.UnregisterInputListener(this);
        PlayerUi.FadeTextOut();
    }

    public static void Generate(Region region)
    {
        if (_saveStonePrefab == null) _saveStonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Save Stone");
        GameObject saveStoneObject = Instantiate(_saveStonePrefab);
        saveStoneObject.GetComponent<SaveStoneBehaviour>().Initialise(region);
    }

    private void Initialise(Region region)
    {
        _region = region;
        transform.position = _region.ShrinePosition;
        PathingGrid.AddBlockingArea(_region.ShrinePosition, 0.5f);
        if (!_region.Saved) return;
        DisableParticles(true);
        Destroy(this);
    }

    private void DisableParticles(bool clear)
    {
        foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>(transform))
        {
            ps.Stop();
            if (clear) ps.Clear();
        }

        foreach (TrailRenderer trailRenderer in transform.GetComponentsInChildren<TrailRenderer>(transform))
        {
            if (clear) trailRenderer.Clear();
            else trailRenderer.DOTime(0, 1);
        }
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.TakeItem) return;
        SaveController.SaveGame();
        DisableParticles(false);
        SaveIconController.Save();
        _region.Saved = true;
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}