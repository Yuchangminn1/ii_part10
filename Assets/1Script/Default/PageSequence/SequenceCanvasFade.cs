using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SequenceCanvasFade : MonoBehaviour
{

    [Tooltip("페이드 효과를 적용할 CanvasGroup")] public CanvasGroup canvasGroup;

    [Tooltip("페이드 효과를 적용할 CanvasGroup")] public Graphic[] graphics;


    [Tooltip("페이드 효과의 지속 시간 (초)")] public float fadeDuration = 1f;

    public bool isPlaying = false;

    Coroutine fadeCoroutine;

    Color color = new Color(1, 1, 1, 0);

    void OnEnable()
    {

        if (graphics != null && graphics.Length > 0)
        {
            foreach (var graphic in graphics)
            {
                if (graphic.color == color)
                {
                    graphic.raycastTarget = false;

                }
            }
        }
    }


    private void Awake()
    {
        // CanvasGroup이 할당되지 않았다면 같은 GameObject에서 가져옵니다.
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        PageSequenceController pageSequenceController = GetComponent<PageSequenceController>();
        if (pageSequenceController != null)
        {
            pageSequenceController.onStartPage += FadeIn;
            pageSequenceController.onEndPage += FadeOut;
            Debug.Log("pageSequenceController Set");
        }
        else
        {
            Debug.Log("pageSequenceController is null");
        }
        graphics = GetComponentsInChildren<Graphic>();

    }

    private void Start()
    {
    }

    // private void OnDisable()
    // {
    //     if (fadeCoroutine != null)
    //     {
    //         if (canvasGroup != null && canvasGroup.alpha == 0)
    //         {
    //             StopCoroutine(fadeCoroutine);
    //             fadeCoroutine = null;
    //             isPlaying = false;
    //         }
    //         
    //     }
    // }

    /// <summary>
    /// CanvasGroup을 완전히 보이게 하는 페이드 인 효과 실행
    /// </summary>
    public void FadeIn(Action _callback)
    {
        Debug.Log("IN");
        if (!isPlaying)
            fadeCoroutine = StartCoroutine(FadeRoutine(canvasGroup.alpha, 1f, _callback));
        isPlaying = true;
    }

    /// <summary>
    /// CanvasGroup을 완전히 숨기는 페이드 아웃 효과 실행
    /// </summary>
    public void FadeOut(Action _callback)
    {
        if (!isPlaying)
        {
            Debug.Log("Out");
            fadeCoroutine = StartCoroutine(FadeRoutine(canvasGroup.alpha, 0f, _callback));
        }

        isPlaying = true;
    }

    /// <summary>
    /// 시작 값에서 목표 값까지 알파값을 선형 보간하여 변경하는 코루틴
    /// </summary>
    /// <param name="startAlpha">시작 알파값</param>
    /// <param name="targetAlpha">목표 알파값</param>
    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, Action _callback)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        Debug.Log($"targetAlpha = {targetAlpha}");

        canvasGroup.alpha = targetAlpha;
        isPlaying = false;
        _callback.Invoke();
    }
}