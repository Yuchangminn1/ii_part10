using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class SubVideoPlayer : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoPlayer nextvideo;

    [SerializeField] bool isLoop;

    [SerializeField] bool isStartPlay;

    [SerializeField] float startDelay = -1;

    public Graphic graphic;


    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    public UnityEvent nextCallBack;

    WaitForSeconds videoStartDelay;

    public bool fin = false;

    WaitForSeconds videoWait = new WaitForSeconds(0.2f);
    public bool isFir = false;

    public UnityEvent onstart;

    WaitForSeconds hintDelay = new WaitForSeconds(2f);
    public bool isStartDelay = false;

    void Awake()
    {
        if (videoPlayer != null) videoPlayer.Prepare();
        if (nextvideo != null) nextvideo.Prepare();
        graphic = GetComponent<Graphic>();
        if (videoPlayer != null) isLoop = videoPlayer.isLooping;
        if (startDelay > 0f)
            videoStartDelay = new WaitForSeconds(startDelay);
    }

    public void Fin()
    {
        fin = true;
    }

    void OnEnable()
    {
        if (isFir) StartSeq();
    }

    public void StartSeq()
    {
        videoPlayer.Prepare();

        if (nextvideo != null) nextvideo.Prepare();


        if (FadeManager.Instance && graphic != null) FadeManager.Instance.SetAlphaZero(graphic);
        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
        }
        fin = false;

        Debug.Log("start play");
        StartCoroutine(OnEnableStart(1));
    }

    IEnumerator OnEnableStart(float _delayTime = -1)
    {
        onstart?.Invoke();

        if (isStartDelay) yield return hintDelay;


        if (_delayTime > 0)
            yield return videoStartDelay;
        videoPlayer.Prepare();

        while (videoPlayer.isPrepared == false)
        {
            Debug.Log("prepare Wait");
            yield return waitForFixedUpdate;
        }
        videoPlayer.Play();
        Debug.Log("Play");
        while (FadeManager.Instance == false)
        {
            yield return waitForFixedUpdate;

        }
        FadeManager.Instance.SetAlphaOne(graphic);

        yield return videoWait;

        while (videoPlayer.isPlaying)
        {
            yield return videoWait;
        }
        nextCallBack?.Invoke();
        FadeManager.Instance.SetAlphaZero(graphic);


    }


}
