using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutFade : MonoBehaviour
{

    public float delayTime = -1;

    WaitForSeconds waitForSeconds;

    public bool iscut;

    Graphic graphic;

    Coroutine coroutine = null;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        if (delayTime > 0) waitForSeconds = new WaitForSeconds(delayTime);
    }

    public void ToggleGrahpic()
    {
        if (coroutine == null) coroutine = StartCoroutine(delayFade());
    }

    void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }


    IEnumerator delayFade()
    {
        if (delayTime > 0f)
            yield return waitForSeconds;
        if (iscut)
        {
            FadeManager.Instance.ToggleCut(graphic);
            yield break;
        }
        FadeManager.Instance.ToggleFade(graphic);
        coroutine = null;
    }
}
