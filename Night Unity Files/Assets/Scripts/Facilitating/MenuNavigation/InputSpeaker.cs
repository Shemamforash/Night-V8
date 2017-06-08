using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSpeaker : MonoBehaviour
{
    private static List<InputListener> listeners = new List<InputListener>();
    private static bool confirmPressed = false, cancelPressed = false;
    private enum Axes { CANCEL, SUBMIT };

    public static void RegisterForInput(InputListener listener)
    {
        listeners.Add(listener);
    }

    private void CheckConfirmPressed()
    {
        if (Input.GetAxis("Submit") != 0)
        {
            if (!confirmPressed)
            {
                confirmPressed = true;
                Broadcast(Axes.SUBMIT);
            }
        }
        else
        {
            confirmPressed = false;
        }
    }

    private void CheckCancelPressed()
    {
        if (Input.GetAxis("Cancel") != 0)
        {
            if (!cancelPressed)
            {
                cancelPressed = true;
                Broadcast(Axes.CANCEL);
            }
        }
        else
        {
            cancelPressed = false;
        }
    }

    private void Broadcast(Axes axis)
    {
        switch (axis)
        {
            case Axes.SUBMIT:
                foreach (InputListener l in listeners)
                {
                    l.OnConfirm();
                }
                break;
            case Axes.CANCEL:
                foreach (InputListener l in listeners)
                {
                    l.OnCancel();
                }
                break;
        }
    }

    public void Update()
    {
        CheckConfirmPressed();
        CheckCancelPressed();
    }
}
