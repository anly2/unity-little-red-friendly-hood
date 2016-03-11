using UnityEngine;
using System.Collections;

public class ButtonListener : MonoBehaviour
{
    public delegate void Action();

    internal string buttonName;
    internal Action action;

    void Update()
    {
        if (Input.GetButtonUp(buttonName))
            action();
    }

    public void Remove()
    {
        Destroy(this);
    }
}

public static class InputExtensions
{
    public static ButtonListener OnButtonDown(this MonoBehaviour script, string buttonName, ButtonListener.Action action)
    {
        ButtonListener listener = script.gameObject.AddComponent<ButtonListener>();

        listener.buttonName = buttonName;
        listener.action = action;

        return listener;
    }

    public static void RemoveButtonListener(this MonoBehaviour script, ButtonListener listener)
    {
        listener.Remove();
    }
}
