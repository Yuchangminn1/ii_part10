using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SetRayDelay : MonoBehaviour
{
    Graphic graphic;

    public float waitDelay = 0.5f;

    WaitForSeconds waitForSeconds;
    Coroutine coroutine;

    public bool rayValue = false;

    void Awake()
    {
        waitForSeconds = new WaitForSeconds(waitDelay);
        graphic = GetComponent<Graphic>();
    }

    void OnEnable()
    {
        if (coroutine == null) coroutine = StartCoroutine(SetRay());
    }

    void OnDisable()
    {
        if (coroutine != null) { StopCoroutine(coroutine); coroutine = null; }
    }
    IEnumerator SetRay()
    {
        yield return waitForSeconds;
        graphic.raycastTarget = rayValue;

        coroutine = null;

    }

}
