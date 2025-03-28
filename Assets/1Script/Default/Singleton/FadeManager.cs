using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 단일 Graphic에 대해 토글 페이드 효과를 적용합니다.
    /// </summary>
    /// <param name="graphic">페이드 효과 적용 대상</param>
    /// <param name="fadeTime">전환 시간 (초, 기본값 1f)</param>
    public void ToggleFade(Graphic graphic, float fadeTime = 1f)
    {
        if (graphic == null)
        {
            Debug.LogWarning("ToggleFade: 적용할 graphic이 null입니다.");
            return;
        }
        // 단일 graphic을 배열로 변환하여 아래 오버로드 호출
        ToggleFade(fadeTime, graphic);
    }

    public void TargetFade(Graphic graphic, float targetAlphas, float fadeTime = 1f)
    {
        if (graphic == null)
        {
            Debug.LogWarning("ToggleFade: 적용할 graphic이 null입니다.");
            return;
        }
        // 단일 graphic을 배열로 변환하여 아래 오버로드 호출
        StartCoroutine(TargetFadeCoroutine(graphic, targetAlphas, fadeTime));
    }

    /// <summary>
    /// 여러 Graphic에 대해 토글 페이드 효과를 동시에 적용합니다.
    /// </summary>
    /// <param name="fadeTime">전환 시간 (초, 기본값 1f)</param>
    /// <param name="graphics">페이드 효과 적용 대상 Graphic 배열</param>
    public void ToggleFade(float fadeTime = 1f, params Graphic[] graphics)
    {
        if (graphics == null || graphics.Length == 0)
        {
            Debug.LogWarning("ToggleFade: 적용할 graphic이 없습니다.");
            return;
        }
        StartCoroutine(ToggleFadeCoroutine(fadeTime, graphics));
    }

    private IEnumerator ToggleFadeCoroutine(float fadeTime, Graphic[] graphics)
    {
        int count = graphics.Length;
        float[] startAlphas = new float[count];
        float[] targetAlphas = new float[count];

        // 각 Graphic에 대해 시작 알파값과 목표 알파값 계산 (현재 알파값이 0.5 이상이면 0, 아니면 1로 전환)
        for (int i = 0; i < count; i++)
        {
            if (graphics[i] != null)
            {
                startAlphas[i] = graphics[i].color.a;
                targetAlphas[i] = startAlphas[i] >= 0.5f ? 0f : 1f;
            }
            else
            {
                startAlphas[i] = 0f;
                targetAlphas[i] = 0f;
            }
        }

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            for (int i = 0; i < count; i++)
            {
                if (graphics[i] != null)
                {
                    Color color = graphics[i].color;
                    color.a = Mathf.Lerp(startAlphas[i], targetAlphas[i], t);
                    graphics[i].color = color;
                }
            }
            yield return null;
        }

        // 최종 알파값 고정
        for (int i = 0; i < count; i++)
        {
            if (graphics[i] != null)
            {
                Color color = graphics[i].color;
                color.a = targetAlphas[i];
                graphics[i].color = color;
                SetGraphicRayTarget(graphics[i]);


            }
        }

    }

    private IEnumerator TargetFadeCoroutine(Graphic graphic, float targetAlphas, float fadeTime)
    {
        float startAlphas;

        // 각 Graphic에 대해 시작 알파값과 목표 알파값 계산 (현재 알파값이 0.5 이상이면 0, 아니면 1로 전환)
        if (graphic == null) yield break;

        startAlphas = graphic.color.a;

        float elapsed = 0f;
        Color color = graphic.color;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            color = graphic.color;
            color.a = Mathf.Lerp(startAlphas, targetAlphas, t);
            graphic.color = color;
            yield return null;
        }

        // 최종 알파값 고정
        color = graphic.color;
        color.a = targetAlphas;
        graphic.color = color;
        SetGraphicRayTarget(graphic);

    }

    public void SetAlphaZero(Graphic graphic)
    {
        Color originalColor = graphic.color;
        graphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        SetGraphicRayTarget(graphic);

    }

    public void SetAlphaOne(Graphic graphic)
    {
        Color originalColor = graphic.color;
        graphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        SetGraphicRayTarget(graphic);
    }

    public void ToggleCut(Graphic graphic)
    {
        bool _flag = false;


        _flag = graphic.color.a > 0.1f;
        Debug.Log($"_flag = {_flag}");
        if (_flag) SetAlphaZero(graphic);
        else SetAlphaOne(graphic);
    }

    public void SetGraphicRayTarget(Graphic graphic)
    {
        string tag = graphic.gameObject.tag;
        if (tag == "mp4")
        {
            graphic.raycastTarget = false;
            return;
        }

        graphic.raycastTarget = graphic.color.a > 0.5f;
    }
}
