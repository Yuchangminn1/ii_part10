using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class al0000 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Graphic graphic;
    public VideoPlayer videoPlayer;
    WaitForSeconds waitforsecond = new WaitForSeconds(1f);
    void Start()
    {
        graphic = GetComponent<Graphic>();
    }

    void OnEnable()
    {
        StartCoroutine(ResetA());
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    IEnumerator ResetA()
    {
        while (true)
        {
            if (videoPlayer.isPlaying == false)
            {
                FadeManager.Instance.SetAlphaZero(graphic);
            }
            yield return waitforsecond;
        }


    }
}
