using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoFadeController : MonoBehaviour
{
    [Header("VideoPlayer 설정")]
    [SerializeField] private VideoPlayer videoPlayer;    // 재생할 VideoPlayer
    [SerializeField] private RenderTexture renderTexture;  // 이미 Canvas의 RawImage에 할당된 렌더 텍스쳐

    [Header("UI 설정")]
    [SerializeField] private RawImage rawImage;       // 캔버스에서 페이드할 RawImage

    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Action onCompleteCallback;

    void Awake()
    {
        // VideoPlayer → RenderTexture 출력
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        renderTexture = videoPlayer.targetTexture;
        rawImage = GetComponent<RawImage>();
        if (renderTexture != null)
            videoPlayer.targetTexture = renderTexture;

        // 드롭된 프레임은 스킵
        videoPlayer.skipOnDrop = true;

        // 초기 상태: 완전 투명 + 비활성
        SetAlpha(0f);
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);



        // 이벤트 구독
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnPlaybackEnded;
    }

    /// <summary>
    /// 페이드 인 → Prepare → Play → 재생 끝나면 페이드 아웃 → onComplete 호출
    /// </summary>
    public void PlayWithFade(Action onComplete = null)
    {
        onCompleteCallback = onComplete;
        if (rawImage != null)
            rawImage.gameObject.SetActive(true);
        StartCoroutine(FadeInAndPrepare());
    }

    public void PlayWithFade()
    {
        if (rawImage != null)
            rawImage.gameObject.SetActive(true);
        StartCoroutine(FadeInAndPrepare());
    }


    private IEnumerator FadeInAndPrepare()
    {
        yield return Fade(0f, 1f);
        videoPlayer.Prepare();

        // 이벤트 미수신 대비 2초간 fallback 대기
        float timer = 0f;
        while (!videoPlayer.isPrepared && timer < 2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!videoPlayer.isPrepared)
            OnPrepared(videoPlayer);
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnPlaybackEnded(VideoPlayer vp)
    {
        StartCoroutine(FadeOutAndCleanup());
    }

    private IEnumerator FadeOutAndCleanup()
    {
        yield return Fade(1f, 0f);
        videoPlayer.Stop();
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (rawImage == null) return;
        var c = rawImage.color;
        c.a = a;
        rawImage.color = c;
    }

    void OnDisable()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnPlaybackEnded;
    }
}