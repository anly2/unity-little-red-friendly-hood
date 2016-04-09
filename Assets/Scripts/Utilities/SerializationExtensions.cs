using System;
using UnityEngine;
using System.Collections;

public static class SerializationExtensions {
    public static V3 serializable(this UnityEngine.Vector3 vector)
    {
        return new V3(vector.x, vector.y, vector.z);
    }

    public static UnityEngine.Vector3 unwarp(this V3 v)
    {
        return new UnityEngine.Vector3(v.x, v.y, v.z);
    }
}

[Serializable]
public struct V3
{
    public float x, y, z;

    public V3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
