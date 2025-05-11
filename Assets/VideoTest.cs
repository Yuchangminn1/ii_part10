using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoTest : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    Coroutine coroutine = null;

    void Awake()
    {
        videoPlayer.Prepare();
        gameObject.SetActive(false);
        videoPlayer.loopPointReached += source => { gameObject.SetActive(false); };

    }
    void OnEnable()
    {
        videoPlayer.Play();
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = StartCoroutine(timeFalse());

    }
    public IEnumerator timeFalse()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
    void OnDisable()
    {
        gameObject.SetActive(false);
        videoPlayer.Stop();
        videoPlayer.Prepare();
        StopCoroutine(coroutine);
        coroutine = null;

    }

}
