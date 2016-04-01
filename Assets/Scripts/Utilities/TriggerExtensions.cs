using UnityEngine;
using System.Collections;


public static class TriggerExtensions
{
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


    public delegate bool VetoingAction();

    public static void WhenInRange(this GameObject actor1, Vector3 position, float radius,
        VetoingAction action)
    {
        GameObject location = new GameObject();
        location.name = "Location Marker";
        location.transform.position = position;

        WhenInRange(actor1, location, radius, action);
    }

    public static void WhenInRange(this GameObject actor1, GameObject actor2, float radius,
        VetoingAction action)
    {
        GameObject trigger = new GameObject();
        trigger.name = "Trigger: " + actor1.name + " within " + radius + " units";
        trigger.transform.parent = actor2.transform;
        trigger.transform.position = actor2.transform.position;

        CircleCollider2D collider = trigger.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = radius;

        OnEnterScript script = trigger.AddComponent<OnEnterScript>();
        script.action = (o) =>
        {
            if (o.gameObject != actor1)
                return;

            if (action())
                MonoBehaviour.Destroy(trigger);
        };
    }

    private class OnEnterScript : MonoBehaviour
    {
        public delegate void Action(Collider2D other);

        public Action action = null;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (action != null)
                action(other);
        }
    }
}
