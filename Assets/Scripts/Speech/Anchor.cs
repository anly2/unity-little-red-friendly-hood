using UnityEngine;
using System.Collections;

public class Anchor : MonoBehaviour {
    public GameObject anchored {
        get { return _anchored; }
        set {
            Canvas canvas = value.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                cam = null;
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera;
            else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                cam = Camera.main;

            _anchored = value;
            lastPosition = toUI(transform.position);
        }
    }

    private Camera cam;
    private GameObject _anchored;
    private Vector3 lastPosition;
    private Vector3 offset;


    public Vector3 toUI(Vector3 position)
    {
        return (cam == null) ? position : cam.WorldToScreenPoint(position);
    }
	
    public static bool isInWorld(GameObject gameObject)
    {
        return (gameObject.layer != LayerMask.NameToLayer("UI"));
    }


	void Update () {
        if (anchored == null)
            return;

        Vector3 position = toUI(this.transform.position);

        Vector3 delta = position - lastPosition;

        anchored.transform.Translate(delta);

        offset += delta;
        lastPosition = position;
	}

    public void UndoOffset()
    {
        anchored.transform.Translate(-offset);
        offset = new Vector3(0, 0, 0);
    }
}
