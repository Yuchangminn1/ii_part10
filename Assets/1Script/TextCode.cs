using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TextCode : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    Graphic graphic;

    Coroutine coroutine;

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    WaitForSeconds waitSecond = new WaitForSeconds(0.15f);

    Action onEndcoroutine;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        onEndcoroutine = ResetCoroutine;
    }

    public void ResetCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }


    }

    void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.Prepare();

        if (graphic != null)
            FadeManager.Instance.SetAlphaZero(graphic);

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeColorA();
        }
    }

    public void ChangeColorA()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
            videoPlayer.Play();
            FadeManager.Instance.SetAlphaOne(graphic);
            if (coroutine == null)
                coroutine = StartCoroutine(StopVideoColor());
        }


    }

    IEnumerator StopVideoColor()
    {
        yield return waitSecond;
        while (true)
        {
            if (!videoPlayer.isPlaying)
            {
                FadeManager.Instance.SetAlphaZero(graphic);

                break;
            }
            yield return waitForFixedUpdate;
        }

        onEndcoroutine?.Invoke();
    }


}
