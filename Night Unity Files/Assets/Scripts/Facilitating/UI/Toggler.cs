using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Toggler : MonoBehaviour
{
    private Text btnText;

    public void Start()
    {
        btnText = transform.Find("Text").GetComponent<Text>();
    }

    public void Toggle()
    {
        if (btnText.text.ToLower() == "on")
        {
            btnText.text = "OFF";
            Off();
        }
        else
        {
            btnText.text = "ON";
            On();
        }
        // EventSystem.current.SetSelectedGameObject(null);
    }

    protected abstract void On();

    protected abstract void Off();
}
