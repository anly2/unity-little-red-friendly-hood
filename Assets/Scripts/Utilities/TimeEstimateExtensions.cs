using UnityEngine;
using System.Collections;

public static class TimeEstimateExtensions {
    private static float timePerChar = 0.25f;

    public static float EstimateReadTime(this string text)
    {
        return 1f + text.Length * timePerChar;
    }


    private static float timePerUnit = 0.05f;

    public static float EstimateTravelTime(this GameObject actor, Vector3 destination, float? speed = null)
    {
        return actor.EstimateTravelTime(actor.transform.position, destination, speed);
    }

    public static float EstimateTravelTime(this GameObject actor, Vector3 position, Vector3 destination, float? speed = null)
    {
        float _speed = speed.HasValue ? speed.Value : actor.GetSpeed();
        float distance = Vector3.Distance(position, destination);

        return (distance * _speed);
    }

    public static float GetSpeed(this GameObject actor)
    {
        // if has a special component
        //      use the value defined on that component

        return timePerUnit;
    }
}
