using UnityEngine;
using System.Collections;

/// <summary>
/// Delayed function call wrapper. All of other overloads follow 
/// the parameter pattern (MonoOwner, Function, Parameters..., DelayInSeconds)
/// </summary>
public sealed class DelayedCall 
{
    /// <summary>
    /// After a delay, Function f will be called.
    /// </summary>
    /// <param name="owner">Mono object that will own the coroutine</param>
    /// <param name="f">Callback function</param>
    /// <param name="delay">A delay in seconds</param>
    public static void Start(MonoBehaviour owner, Function f, float delay)
    {
        owner.StartCoroutine(CallbackCoroutine(f, delay));
    }

    /// <summary>
    /// After a delay, Function f will be called with parameter p1.
    /// </summary>
    /// <param name="owner">Mono object that will own the coroutine</param>
    /// <param name="f">Callback function</param>
    /// <param name="p1">Parameter 1</param>
    /// <param name="delay">A delay in seconds</param>
    public static void Start<P1>(MonoBehaviour owner, Function1<P1> f, P1 p1, float delay)
    {
        owner.StartCoroutine(CallbackCoroutine(f, p1, delay));
    }

    /// <summary>
    /// After a delay, Function f will be called with parameters p1, p2.
    /// </summary>
    /// <param name="owner">Mono object that will own the coroutine</param>
    /// <param name="f">Callback function</param>
    /// <param name="p1">Parameter 1</param>
    /// <param name="p2">Parameter 2</param>
    /// <param name="delay">A delay in seconds</param>
    public static void Start<P1, P2>(MonoBehaviour owner, Function2<P1, P2> f, P1 p1, P2 p2, float delay)
    {
        owner.StartCoroutine(CallbackCoroutine(f, p1, p2, delay));
    }

    /// <summary>
    /// After a delay, Function f will be called with parameters p1, p2, p3.
    /// </summary>
    /// <param name="owner">Mono object that will own the coroutine</param>
    /// <param name="f">Callback function</param>
    /// <param name="p1">Parameter 1</param>
    /// <param name="p2">Parameter 2</param>
    /// <param name="p3">Parameter 3</param>
    /// <param name="delay">A delay in seconds</param>
    public static void Start<P1, P2, P3>(MonoBehaviour owner, Function3<P1, P2, P3> f, P1 p1, P2 p2, P3 p3, float delay)
    {
        owner.StartCoroutine(CallbackCoroutine(f, p1, p2, p3, delay));
    }

    /// <summary>
    /// After a delay, Function f will be called with parameters p1, p2, p3, p4.
    /// </summary>
    /// <param name="owner">Mono object that will own the coroutine</param>
    /// <param name="f">Callback function</param>
    /// <param name="p1">Parameter 1</param>
    /// <param name="p2">Parameter 2</param>
    /// <param name="p3">Parameter 3</param>
    /// <param name="p3">Parameter 4</param>
    /// <param name="delay">A delay in seconds</param>
    public static void Start<P1, P2, P3, P4>(MonoBehaviour owner, Function4<P1, P2, P3, P4> f, P1 p1, P2 p2, P3 p3, P4 p4, float delay)
    {
        owner.StartCoroutine(CallbackCoroutine(f, p1, p2, p3, p4, delay));
    }


    public delegate void Function();
    public delegate void Function1<P1>(P1 p1);
    public delegate void Function2<P1, P2>(P1 p1, P2 p2);
    public delegate void Function3<P1, P2, P3>(P1 p1, P2 p2, P3 p3);
    public delegate void Function4<P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4);

    //----Nothing public beyond here!
    private static IEnumerator CallbackCoroutine(Function f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f();
    }
    private static IEnumerator CallbackCoroutine<P1>(Function1<P1> f, P1 p1, float delay)
    {
        yield return new WaitForSeconds(delay);
        f(p1);
    }
    private static IEnumerator CallbackCoroutine<P1, P2>(Function2<P1, P2> f, P1 p1, P2 p2, float delay)
    {
        yield return new WaitForSeconds(delay);
        f(p1, p2);
    }
    private static IEnumerator CallbackCoroutine<P1, P2, P3>(Function3<P1, P2, P3> f, P1 p1, P2 p2, P3 p3, float delay)
    {
        yield return new WaitForSeconds(delay);
        f(p1, p2, p3);
    }
    private static IEnumerator CallbackCoroutine<P1, P2, P3, P4>(Function4<P1, P2, P3, P4> f, P1 p1, P2 p2, P3 p3, P4 p4, float delay)
    {
        yield return new WaitForSeconds(delay);
        f(p1, p2, p3, p4);
    }
}