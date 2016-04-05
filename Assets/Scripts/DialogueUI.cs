using UnityEngine;
using System.Collections;

public static class DialogueUI {

    private static GameObject _canvas = null;

    public static GameObject canvas
    {
        get
        {
            if (_canvas != null)
                return _canvas;

            _canvas = GameObject.Find("Dialogue Canvas");

            if (_canvas != null)
                return _canvas;

            _canvas = Create("UI/Dialogue/Canvas");
            return _canvas;
        }
    }


    public static GameObject Load(string resourceURI)
    {
        if (!resourceURI.StartsWith("UI/Dialogue/") && !resourceURI.StartsWith("/"))
            resourceURI = "UI/Dialogue/" + resourceURI;

        return Resources.Load(resourceURI) as GameObject;
    }

    public static GameObject Create(string resourceURI, string childName = null)
    {
        GameObject res = Load(resourceURI);

        GameObject obj = GameObject.Instantiate(res);
        obj.name = childName ?? res.name;

        return obj;
    }


    public static GameObject AddChild(this GameObject parent, string resourceURI,
        string childName = null)
    {
        return AddChild(parent.transform, resourceURI, childName);
    }

    public static GameObject AddChild(this Transform parent, string resourceURI,
        string childName = null)
    {
        GameObject child = Create(resourceURI);
        child.transform.SetParent(parent, false);

        if (childName != null)
            child.name = childName;

        return child;
    }


    public static GameObject AddChildIfNotExist(this GameObject parent, string resourceURI,
        string childName = null)
    {
        return AddChildIfNotExist(parent.transform, resourceURI, childName);
    }

    public static GameObject AddChildIfNotExist(this Transform parent, string resourceURI,
        string childName = null)
    {
        if (childName == null)
            childName = Load(resourceURI).name;

        Transform c = parent.Find(childName);

        if (c != null)
            return c.gameObject;

        GameObject child = parent.AddChild(resourceURI);
        child.name = childName;

        return child;
    }
}
