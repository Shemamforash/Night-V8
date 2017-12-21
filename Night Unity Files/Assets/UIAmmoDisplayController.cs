using Facilitating.UI.Elements;
using UnityEngine;

public class UIAmmoDisplayController : MonoBehaviour
{
    public EnhancedText PistolText, RifleText, ShotgunText, SMGText, LMGText;
    private static UIAmmoDisplayController _instance;

    public void Awake()
    {
        _instance = this;
    }

    public static UIAmmoDisplayController Instance()
    {
        return _instance;
    }

    public void SetPistolText(string quantity)
    {
        PistolText.Text(quantity);
    }

    public void SetRifleText(string quantity)
    {
        RifleText.Text(quantity);
    }

    public void SetShotgunText(string quantity)
    {
        ShotgunText.Text(quantity);
    }

    public void SetSmgText(string quantity)
    {
        SMGText.Text(quantity);
    }

    public void SetLmgText(string quantity)
    {
        LMGText.Text(quantity);
    }
}