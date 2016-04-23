using System;
using UnityEngine;
using System.Collections;

public static class SerializationExtensions {
    public static VS3 serializable(this UnityEngine.Vector3 vector)
    {
        return new VS3(vector.x, vector.y, vector.z);
    }
}

[Serializable]
public struct VS3
{
    public float x, y, z;

    public VS3(Vector3 v) : this(v.x, v.y, v.z) {}

    public VS3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public UnityEngine.Vector3 unwarp()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}
