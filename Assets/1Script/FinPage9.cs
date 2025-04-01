using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FinPage9 : MonoBehaviour
{
    public UnityEvent finEvent;

    Coroutine coroutine;

    void OnEnable()
    {

    }

    public void StartFin()
    {
        if (coroutine == null) coroutine = StartCoroutine(DelayToFin());
    }

    void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator DelayToFin()
    {
        yield return new WaitForSeconds(4f);

        finEvent?.Invoke();
        coroutine = null;

    }
}
