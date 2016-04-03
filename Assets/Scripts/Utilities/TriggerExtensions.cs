using UnityEngine;
using System;
using System.Linq;


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
        Aura aura = null;
        aura = actor2.AddAura(radius,
            (o) => { if (action()) MonoBehaviour.Destroy(aura.gameObject); },
            null, actor1);
        aura.gameObject.name = "Trigger: " + actor1.name + " within " + radius + " units";
    }


    public static Aura AddAura(this GameObject actor, float radius,
        Action<Collider2D> onEnter, Action<Collider2D> onExit,
        params GameObject[] affectedActors)
    {
        return AddAura(actor, radius, onEnter, onExit,
            (o) => affectedActors.Contains(o.gameObject));
    }

    public static Aura AddAura(this GameObject actor, float radius,
        Action<Collider2D> onEnter, Action<Collider2D> onExit,
        Aura.ImmunityCheck shouldAffect = null)
    {
        GameObject aura = new GameObject("Aura");
        aura.transform.position = actor.transform.position;
        aura.transform.parent = actor.transform;

        CircleCollider2D collider = aura.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = radius;

        Aura script = aura.AddComponent<Aura>();
        script.onEnter = onEnter;
        script.onExit = onExit;
        script.shouldAffect = shouldAffect;

        return script;
    }

    public class Aura : MonoBehaviour
    {
        public Action<Collider2D> onEnter = null;
        public Action<Collider2D> onExit = null;

        public delegate bool ImmunityCheck(Collider2D actorCollider);
        public ImmunityCheck shouldAffect = null;


        void OnTriggerEnter2D(Collider2D other)
        {
            if (onEnter == null)
                return;

            if (shouldAffect == null || shouldAffect(other))
                onEnter(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (onExit == null)
                return;

            if (shouldAffect == null || shouldAffect(other))
                onExit(other);
        }

        public bool isAffecting(GameObject actor)
        {
            Collider2D a = GetComponent<Collider2D>();

            foreach (Collider2D c in actor.GetComponents<Collider2D>())
                if (shouldAffect == null || shouldAffect(c))
                    if (a.IsTouching(c))
                        return true;

            return false;
        }
    }
}
