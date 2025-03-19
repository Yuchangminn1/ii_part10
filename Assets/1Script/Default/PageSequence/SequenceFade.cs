using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SequenceFade : SequenceScript
{
    [Tooltip("페이드 효과를 적용할 Graphic 배열 (예: UI 이미지 등)")]
    public List<Graphic> graphics;

    [Tooltip("각 Graphic에 대해 true이면 페이드 인, false이면 페이드 아웃")] //Color A 가 true면 시작할때 1 false 면 0
    public bool[] originColorA;

    [Tooltip("페이드 효과의 지속 시간 (초)")] public float fadeDuration = 1f;

    public bool isfading;


    protected override void AwakeSetup()
    {
        if (startDelay > 0f && _startDelay == null)
            _startDelay = new WaitForSeconds(startDelay);
        Initialize();
    }

    private void Initialize()
    {
        if (graphics.Count < 1) graphics.Add(GetComponent<Graphic>());
        if (originColorA == null || originColorA.Length < 1)
        {
            originColorA = new bool[graphics.Count];
            for (int i = 0; i < graphics.Count; i++)
            {
                originColorA[i] = graphics[i].color.a > 0.5f;
            }
        }
    }

    void OnEnable()
    {
        ResetAlphas();

    }

    protected override IEnumerator RunSequence()
    {

        Debug.Log("페이드 RunSequence");

        if (!IsCanFade()) yield break;


        if (_startDelay != null) yield return _startDelay;


        StartFadeEffect();

        // 모든 페이드 효과가 완료될 때까지 기다립니다.
        yield return new WaitForSeconds(fadeDuration);
        isfading = false;
    }

    private bool IsCanFade()
    {
        if (isfading)
        {
            Debug.Log("이미 실행중 ");
            return false;
        }

        if (graphics == null || originColorA == null || graphics.Count != originColorA.Length)
        {
            Debug.LogError("graphics 배열과 fadeInFlags 배열의 길이가 일치하지 않습니다.");
            return false;
        }

        return true;
    }

    private void StartFadeEffect()
    {
        isfading = true;
        if (graphics.Count < 1)
        {
            Debug.Log("graphics is Null ");
            return;
        }
        // 모든 그래픽에 대해 페이드 효과를 동시에 시작합니다.
        for (int i = 0; i < graphics.Count; i++)
        {
            FadeManager.Instance.ToggleFade(graphics[i]);
        }
    }

    public void ResetAlphas()
    {
        if (isInitialize)
        {

            for (int i = 0; i < graphics.Count; i++)
            {
                if (originColorA[i])
                {
                    FadeManager.Instance.SetAlphaOne(graphics[i]);
                }
                else
                {
                    FadeManager.Instance.SetAlphaZero(graphics[i]);
                }
            }
        }

    }
}