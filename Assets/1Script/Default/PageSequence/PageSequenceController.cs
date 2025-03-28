using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SequenceCanvasFade))]
[RequireComponent(typeof(SetColorA))]
public class PageSequenceController : MonoBehaviour
{
    [Tooltip("페이지 번호 (예: 0, 1, 2, ... )")] public int pageNumber;

    [Tooltip("이 페이지에서 실행할 시퀀스 스크립트들 (순서대로 실행됩니다.)")]
    [SerializeField] private SequenceScript[] sequenceScripts;

    Coroutine coroutine;

    [SerializeField] private int currentindex;

    [SerializeField] int nextPageNumber = -1;

    public bool endToNext = false;

    public Action<Action> onStartPage;
    public Action<Action> onEndPage;

    [SerializeField] float defalutResetTime = 8;

    [SerializeField] float resetTime = 8f;

    [SerializeField] bool isMouseDown = false;

    public int CurrentIndex
    {
        get { return currentindex; }
        set
        {
            if (sequenceScripts.Length == 0) return;

            if (sequenceScripts.Length > value)
            {
                currentindex = value;
                RunSequence();
            }

            else if (endToNext)
            {
                //Debug.Log("NextPage");
                PageSequenceManager.Instance.NextPage();
            }
        }
    }

    private void Awake()
    {
        // 매니저에 자신의 컨트롤러 등록 >> 옵저버 
        if (PageSequenceManager.Instance != null)
            PageSequenceManager.Instance.RegisterController(this);

        sequenceScripts = GetComponentsInChildren<SequenceScript>();
        sequenceScripts = sequenceScripts.OrderBy(script => script.currentIndex).ToArray();
        resetTime = defalutResetTime;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            resetTime = defalutResetTime;

        }
    }

    void FixedUpdate()
    {


        if (PageController.Instance.CurrentPage == 0 || isMouseDown) return;
        resetTime -= Time.deltaTime;
        if (resetTime < 0f)
        {
            resetTime = defalutResetTime;
            PageController.Instance.CurrentPage = 0;
        }
    }

    public void ChangePage()
    {
        if (onStartPage != null)
        {
            onStartPage.Invoke(() => SequenceStart());

            //Debug.Log($"pageNumber = {pageNumber} currentindex = {currentindex}");
        }
    }

    public void SequenceStart()
    {
        //Debug.Log("SequenceStart");
        CurrentIndex = 0;
    }

    public void SequenceEnd()
    {
        //Debug.Log("SequenceEnd");
        if (endToNext) PageSequenceManager.Instance.NextPage();
    }





    public void RunSequence()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        //Debug.Log($"Start Page{pageNumber} Sequence{currentindex}");
        if (sequenceScripts.Length == 0) return;

        coroutine = StartCoroutine(sequenceScripts[currentindex].StartSequence());

    }

    public void CurrentIndexTriggerON()
    {
        if (sequenceScripts == null || sequenceScripts.Length < 1) return;
        sequenceScripts[currentindex].TriggerOn();
    }


}