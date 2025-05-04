using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BacktoStartVideoScript : MonoBehaviour
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
        StartCoroutine(SetEvent());
    }

    IEnumerator SetEvent()
    {
        while (CustomSerialController.Instance == null)
        {
            yield return waitForFixedUpdate;
        }
        CustomSerialController.Instance.userMissButtonEvent.AddListener(MissAction);
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

        if (graphic != null && FadeManager.Instance)
            FadeManager.Instance.SetAlphaZero(graphic);

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            MissAction();
        }
    }

    public void MissAction()
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
