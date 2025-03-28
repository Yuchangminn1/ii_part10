using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceFade : SequenceScript
{
    [Tooltip("페이드 효과를 적용할 Graphic 배열 (예: UI 이미지 등)")]
    public List<Graphic> graphics;

    [Tooltip("페이드 효과의 지속 시간 (초)")] public float fadeDuration = 1f;


    public bool isCut;


    protected override void AwakeSetup()
    {
        if (startDelay > 0f && _startDelay == null)
            _startDelay = new WaitForSeconds(startDelay);
        Initialize();
    }

    private void Initialize()
    {
        if (graphics.Count < 1) graphics.Add(GetComponent<Graphic>());

    }

    protected override IEnumerator RunSequence()
    {

        //Debug.Log("페이드 RunSequence");

        if (_startDelay != null) yield return _startDelay;

        if (isCut)
        {
            StartCutEffect();
        }

        else
        {
            StartFadeEffect();
        }

        // 모든 페이드 효과가 완료될 때까지 기다립니다.
        yield return new WaitForSeconds(fadeDuration);
    }



    private void StartFadeEffect()
    {
        if (graphics.Count < 1)
        {
            //Debug.Log("graphics is Null ");
            return;
        }
        // 모든 그래픽에 대해 페이드 효과를 동시에 시작합니다.
        for (int i = 0; i < graphics.Count; i++)
        {
            FadeManager.Instance.ToggleFade(graphics[i]);
        }
    }

    private void StartCutEffect()
    {
        if (graphics.Count < 1)
        {
            //Debug.Log("graphics is Null ");
            return;
        }

        for (int i = 0; i < graphics.Count; i++)
        {
            FadeManager.Instance.ToggleCut(graphics[i]);
        }
        Debug.Log("QQQQQQ");

    }

}