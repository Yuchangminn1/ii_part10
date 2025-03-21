using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StampScript : MonoBehaviour
{
    Graphic[] graphics;
    int stamp = 0;

    WaitForSeconds waitForSeconds = new WaitForSeconds(0.4f);

    Coroutine coroutine = null;

    void Awake()
    {
        graphics = GetComponentsInChildren<Graphic>();
    }


    public void StartStamp()
    {
        stamp = ScoreManager.Instance.stamp;
        if (stamp > 0 && coroutine == null)
        {
            coroutine = StartCoroutine(StampCoroutine());
        }
    }

    IEnumerator StampCoroutine()
    {
        for (int i = 0; i < stamp; i++)
        {
            FadeManager.Instance.TargetFade(graphics[i], 1f, 0.3f);
            yield return waitForSeconds;
        }
        coroutine = null;

    }
}
