using UnityEngine;
using System.Collections;

public static class CameraExtensions
{
    public static IEnumerator MotionTo(this Camera camera, Vector2 target, float duration)
    {
        Vector3 end = new Vector3(target.x, target.y, camera.transform.position.z);
        return camera.MotionTo(end, duration);
    }
    
    public static IEnumerator MotionTo(this Camera camera, Vector3 target, float duration)
    {
        Vector3 start = camera.transform.position;
        float distance = Vector3.Distance(start, target);

        return new Tween(delegate (float p) {
            camera.transform.position = Vector3.MoveTowards(start, target, p * distance);
        }, duration);
    }
}
