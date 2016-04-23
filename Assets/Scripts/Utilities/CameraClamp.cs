using UnityEngine;
using System;

public class CameraClamp : MonoBehaviour {

    public GameObject boundingBox;

    private Camera cam;
    private Bounds bounds;
    private Vector3 offset;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
            throw new InvalidOperationException("The CameraClamp component must be attached to a Camera.");

        bounds = GetBounds(boundingBox);
    }

    public static Bounds GetBounds(GameObject o)
    {
        Collider c = o.GetComponent<Collider>();
        if (c != null)
            return c.bounds;

        Renderer r = o.GetComponent<Renderer>();
        if (r != null)
            return r.bounds;

        return new Bounds(o.transform.position, o.transform.localScale);
    }
	

	void Update () {
        transform.Translate(-offset);
        offset = GetExcess();
        transform.Translate(offset);
	}


    private static Vector3 V0 = new Vector3(0, 0, 0);
    Vector3 GetExcess()
    {
        if (bounds.Contains(cam.transform.position))
            return V0;

        return bounds.ClosestPoint(cam.transform.position) - cam.transform.position;
    }
}
