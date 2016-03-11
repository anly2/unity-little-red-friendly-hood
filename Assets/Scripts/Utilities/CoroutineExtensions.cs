using UnityEngine;
using System.Collections;

public static class CoroutineExtensions
{
    public delegate void Action();
    public delegate object Expression();


    public static IEnumerator Then(this IEnumerator self, IEnumerator then)
    {
        while (self.MoveNext())
            yield return self.Current;

        while (then.MoveNext())
            yield return then.Current;
    }

    public static IEnumerator Then(this IEnumerator self, Action then)
    {
        return self.Then(() => { then(); return null; });
    }

    public static IEnumerator Then(this IEnumerator self, Expression then)
    {
        while (self.MoveNext())
            yield return self.Current;

        yield return then();
    }


    public static IEnumerator ButFirst(this IEnumerator self, IEnumerator first)
    {
        while (first.MoveNext())
            yield return first.Current;

        while (self.MoveNext())
            yield return self.Current;
    }

    public static IEnumerator ButFirst(this IEnumerator self, Action first)
    {
        return self.ButFirst(() => { first(); return null; });
    }

    public static IEnumerator ButFirst(this IEnumerator self, Expression first)
    {
        yield return first();

        while (self.MoveNext())
            yield return self.Current;
    }


    public static IEnumerator Then(this IEnumerator self, YieldInstruction then)
    {
        while (self.MoveNext())
            yield return self.Current;

        yield return then;
    }

    public static IEnumerator Then(this YieldInstruction self, IEnumerator then)
    {
        yield return self;

        while (then.MoveNext())
            yield return then.Current;
    }


    public static IEnumerator Then(this YieldInstruction self, YieldInstruction then)
    {
        yield return self;
        yield return then;
    }

    public static IEnumerator Then(this YieldInstruction self, Action then)
    {
        return self.Then(() => { then(); return null; });
    }

    public static IEnumerator Then(this YieldInstruction self, Expression then)
    {
        yield return self;
        yield return then();
    }

    
    public static Coroutine Start(this IEnumerator coroutine, MonoBehaviour script)
    {
        return script.StartCoroutine(coroutine);
    }

    public static void Stop(this Coroutine coroutine, MonoBehaviour script)
    {
        script.StopCoroutine(coroutine);
    }
}
