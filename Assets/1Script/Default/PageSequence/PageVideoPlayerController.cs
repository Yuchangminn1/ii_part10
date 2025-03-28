using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PageVideoPlayerController : SequenceScript
{
    [Header("비디오 플레이어 설정")]
    [Tooltip("비디오 재생에 사용할 VideoPlayer 컴포넌트")]
    public VideoPlayer videoPlayer;

    [Tooltip("렌더 텍스쳐가 들어있는 RawImage ")]
    public RawImage targetRawImage;
    [Tooltip("영상 실행할때 페이드 시킬 그래픽 ")]
    public Graphic[] toggleGraphics;


    //플레이 판정 기다려줄 딜레이
    //WaitForSeconds delay = new WaitForSeconds(0.15f);

    Coroutine colorCoroutine;

    [SerializeField] bool isLoop = false;

    [SerializeField] bool isendFade = false;


    public float fadeTime = 1f;

    WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);


    protected override void AwakeSetup()
    {
        _startDelay = new WaitForSeconds(startDelay);
        targetRawImage = GetComponent<RawImage>();
        // VideoPlayer가 할당되지 않았다면 같은 GameObject에서 가져오기
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        // 페이지에 진입할 때, 조건에 따라 비디오 초기화 및 자동 재생
        if (videoPlayer != null && videoPlayer.isActiveAndEnabled)
        {
            videoPlayer.Prepare();
        }
    }

    private void OnDisable()
    {
        // 페이지를 떠날 때 비디오를 정지하고 초기화
        if (colorCoroutine != null)
        {
            StopCoroutine(colorCoroutine);
            colorCoroutine = null;
        }
    }

    public void ToogleGraphic()
    {

        if (toggleGraphics == null || toggleGraphics.Length < 1) return;
        for (int i = 0; i < toggleGraphics.Length; i++)
        {
            FadeManager.Instance.ToggleFade(toggleGraphics[i], 0.05f);
        }
    }

    protected override IEnumerator RunSequence()
    {
        //Debug.Log("RunSequence");
        if (startDelay > 0f)
            yield return _startDelay;


        if (videoPlayer == null)
        {
            Debug.Log("videoPlayer is Null");
            yield break;
        }
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        if (targetRawImage != null)
        {
            FadeManager.Instance.ToggleFade(fadeTime, targetRawImage);
        }

        while (targetRawImage.color.a < 0.9f)
        {
            yield return null;
        }

        ToogleGraphic();
        while (videoPlayer.isPlaying && !isLoop)
        {
            yield return null;
        }
        if (isendFade)
        {
            FadeManager.Instance.ToggleFade(targetRawImage);
            yield return waitForSeconds;
        }
    }
}