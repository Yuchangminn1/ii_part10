using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VIdeoFadeOnOFf : MonoBehaviour
{
    Graphic graphic;
    public VideoPlayer videoPlayer;
    WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
    void Awake()
    {
        if (graphic == null) graphic = GetComponent<Graphic>();

    }
    public void PlayVideoFade()
    {
        FadeManager.Instance.SetAlphaOne(graphic);

        videoPlayer.Prepare();
    }

    IEnumerator PlayCoroutine()
    {
        videoPlayer.Prepare();
        yield return waitForSeconds;

        while (videoPlayer.isPrepared == false)
        {
            yield return waitForSeconds;
        }

        videoPlayer.Play();
        yield return waitForSeconds;

        FadeManager.Instance.SetAlphaOne(graphic);

        while (videoPlayer.isPlaying)
        {
            yield return waitForSeconds;

        }

        FadeManager.Instance.SetAlphaZero(graphic);


    }
}
