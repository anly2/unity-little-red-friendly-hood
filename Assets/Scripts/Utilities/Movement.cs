using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    [Tooltip("The speed of the actor in 'units per second'")]
    public float speed;

}

public static class SpeedExtensions
{
    private static float unitsPerSecond = 2f;

    public static float GetSpeed(this GameObject actor)
    {
        Movement m = actor.GetComponent<Movement>();

        if (m != null)
            return m.speed;

        return unitsPerSecond;
    }

    public static void SetSpeed(this GameObject actor, float speed)
    {
        Movement m = actor.GetComponent<Movement>();

        if (m == null)
            m = actor.AddComponent<Movement>();

        m.speed = speed;
    }
}