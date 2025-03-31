using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Page6TimmerScript : SequenceScript
{
    [Header("현재 순서 번호 ,질문 부모 오브젝트")]
    public GameObject curTarget;
    [Header("이전 퀴즈 질문 , 답 부모 오브젝트")]
    public GameObject PreTarget;
    List<Graphic> graphics;

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
        if (graphics == null) graphics = new List<Graphic>();

        if (curTarget == null) curTarget = gameObject;

        if (curTarget != null) graphics.Add(curTarget.GetComponentInChildren<Graphic>());

        if (PreTarget != null) graphics.Add(PreTarget.GetComponentInChildren<Graphic>());
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