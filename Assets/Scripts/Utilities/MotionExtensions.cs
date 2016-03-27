using UnityEngine;
using System.Collections;

public static class MotionExtensions
{
    public static IEnumerator MotionTo(this GameObject actor, Vector2 target, float duration)
    {
        Vector3 end = new Vector3(target.x, target.y, actor.transform.position.z);
        return actor.MotionTo(end, duration);
    }
    
    public static IEnumerator MotionTo(this GameObject actor, Vector3 target, float duration)
    {
        Vector3 start = actor.transform.position;
        float distance = Vector3.Distance(start, target);

        return new Tween(delegate (float p) {
            actor.transform.position = Vector3.MoveTowards(start, target, p * distance);
        }, duration);
    }
}
