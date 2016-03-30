using UnityEngine;
using System.Collections;

public class Anchor : MonoBehaviour {

    delegate Vector3 Transformation(Vector3 point);


    public GameObject anchored {
        get { return _anchored; }
        set {
            _anchored = value;
            lastPosition = transform.position;
        }
    }

    private GameObject _anchored;
    private Vector3 lastPosition;


    public static Vector3 toUI(Vector3 position)
    {
        return Camera.main.WorldToScreenPoint(position);
    }
	
    public static bool isInWorld(GameObject gameObject)
    {
        return (gameObject.layer != LayerMask.NameToLayer("UI"));
    }


	void Update () {
        if (anchored == null)
        {
            Destroy(this);
            return;
        }

        Vector3 position = this.transform.position;

        if (lastPosition == null)
            lastPosition = position;

        if (lastPosition == position)
            return;

        Vector3 offset = toUI(position) - toUI(lastPosition);

        anchored.transform.position = anchored.transform.position + offset;

        lastPosition = position;
	}
}
