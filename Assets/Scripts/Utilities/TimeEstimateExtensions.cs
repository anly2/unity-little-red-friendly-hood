using UnityEngine;
using System.Collections;

public static class TimeEstimateExtensions {

    public static float EstimateReadTime(this string text)
    {
        return Mathf.Max(2, 8 * Mathf.Log10(text.Length / 10));
    }


    public static float EstimateTravelTime(this GameObject actor, Vector3 destination, float? speed = null)
    {
        return actor.EstimateTravelTime(actor.transform.position, destination, speed);
    }

    public static float EstimateTravelTime(this GameObject actor, Vector3 position, Vector3 destination, float? speed = null)
    {
        float _speed = speed ?? actor.GetSpeed();
        float distance = Vector3.Distance(position, destination);

        return (distance / _speed);
    }
}
