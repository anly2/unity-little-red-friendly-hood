using UnityEngine;
using System.Collections;
using System;

public delegate void Setter(float percent);

public class Animation : IEnumerator {
    private IEnumerator kernel;


    public Animation (Setter setter, float duration)
    {
        kernel = GetEnumeration(setter, duration);
    }

    public Animation(float duration, Setter setter) : this(setter, duration) {}

    
    public static IEnumerator Empty { get { yield break; } }


    private static IEnumerator GetEnumeration(Setter setter, float duration)
    {
        if (duration < Mathf.Epsilon)
        {
            setter(1f);
            yield break;
        }

        float p = 0f;
        float speed = 1f / duration;

        while (p < 1f)
        {
            p += Time.deltaTime * speed;
            setter(p);
            yield return p;
        }

        setter(1f);
        yield break;
    }


    /* Proxy methods */

    public object Current
    {
        get
        {
            return kernel.Current;
        }
    }


    public bool MoveNext()
    {
        return kernel.MoveNext();
    }

    public void Reset()
    {
        kernel.Reset();
    }
}
