using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class SubVideoPlayer : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    [SerializeField] bool isLoop;

    [SerializeField] bool isStartPlay;

    [SerializeField] float startDelay = -1;

    Graphic graphic;

    Coroutine videoStartC = null;


    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    WaitForSeconds coroutineStartDelay = new WaitForSeconds(0.15f);

    WaitForSeconds videoStartDelay;

    [SerializeField] UnityEvent onVideoEnd;

    public bool fin = false;

    void Awake()
    {
        if (videoPlayer != null) videoPlayer.Prepare();
        graphic = GetComponent<Graphic>();
        isLoop = videoPlayer.isLooping;
        if (startDelay > 0f)
            videoStartDelay = new WaitForSeconds(startDelay);
    }

    public void Fin()
    {
        fin = true;
    }

    void OnEnable()
    {
        fin = false;
        if (isStartPlay)
        {
            StartCoroutine(OnEnableStart());
        }
    }

    IEnumerator OnEnableStart()
    {
        yield return videoStartDelay;
        VideoPlay();
    }



    public void VideoPlay()
    {
        Debug.Log($"VideoPlay {name}");
        if (videoStartC == null) videoStartC = StartCoroutine(CVideoPlay());
        else
        {
            Debug.Log($"Coroutine Is Not Null {name}");

        }
    }

    public void CoroutineEnd()
    {
        StartCoroutine(CoroutineEndC());
    }

    IEnumerator CoroutineEndC()
    {
        if (videoStartC == null) yield break;
        StopCoroutine(videoStartC);
        videoStartC = null;
    }

    IEnumerator CVideoPlay()
    {
        if (videoPlayer == null)
        {
            CoroutineEnd();
            yield break;
        }
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return waitForFixedUpdate;
        }
        videoPlayer.Play();
        //if (graphic != null) FadeManager.Instance.SetAlphaOne(graphic);
        if (graphic != null) FadeManager.Instance.TargetFade(graphic, 1f, 0.3f);


        yield return coroutineStartDelay;

        while (videoPlayer.isPlaying && !fin)
        {
            yield return waitForFixedUpdate;
        }
        //if (graphic != null) FadeManager.Instance.SetAlphaZero(graphic);
        if (graphic != null) FadeManager.Instance.TargetFade(graphic, 0f, 0.3f);

        onVideoEnd?.Invoke();

        CoroutineEnd();
    }


}
