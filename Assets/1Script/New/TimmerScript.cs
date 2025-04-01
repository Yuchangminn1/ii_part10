using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimmerScript : MonoBehaviour
{
    //배속추가 
    Text text;

    [SerializeField] Text isReadyText;

    [SerializeField] float defaultTimmer = 10f;
    [SerializeField] float defaultRedayTimmer = 3f;

    [SerializeField] float remainTime = 0f;

    public float timeSpeed = 10f;

    //[SerializeField] bool isEnd = false;

    [SerializeField] UnityEvent time5SecEvent;

    [SerializeField] UnityEvent endEvent;


    [Header("해당 페이지 내 존재하는 스크립트 할당 해야함")]
    [SerializeField] AnswerSelector answerSelector;


    bool isEvent;

    Color colorB;

    bool isTimeOver;

    WaitForSeconds delay = new WaitForSeconds(3f);

    Coroutine coroutine;

    public int currentIndex = 1;

    public bool isReady = false;

    //public Graphic[] toggleGraphic;

    public Image timmerImage;


    void Awake()
    {
        text = GetComponent<Text>();
    }
    void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    void OnEnable()
    {
        currentIndex = 0;
        text.text = "";
        isTimeOver = false;
        IsReady();

    }

    void FixedUpdate()
    {
        if (isTimeOver) return;
        remainTime -= Time.deltaTime * timeSpeed;
        if (remainTime < 0f)
        {
            //FadeManager.Instance.SetAlphaZero(text);
            text.text = "";
            remainTime = 0f;
            if (isReady == false)
            {
                FadeManager.Instance.SetAlphaZero(isReadyText);
                isTimeOver = true;


                // redayEvent?.Invoke();
                text.text = $"{(int)Math.Ceiling(remainTime)}";
                FadeManager.Instance.TargetFade(text, 0f);

                if (answerSelector.qSize > currentIndex)
                {
                    ResetTimmer();
                }

                return;
            }

            text.text = "";
            colorB = text.color;
            colorB.a = 0f;
            text.color = colorB;
            isTimeOver = true;
            // if (timmerEndEvent != null)
            //     timmerEndEvent?.Invoke();

            Debug.Log("Answer2");
            FadeManager.Instance.SetAlphaZero(timmerImage);


            answerSelector.Answer();
            //GraphicReset();

            if (answerSelector.qSize > currentIndex)
            {
                ResetTimmer();
            }
            else
            {
                Debug.Log("endEvent");
                endEvent?.Invoke();

            }

            return;

        }
        if (!isEvent && remainTime < 6f && remainTime > 4f)
        {
            isEvent = true;
            time5SecEvent?.Invoke();
        }
        text.text = $"{(int)Math.Ceiling(remainTime)}";
    }



    // public void GraphicReset()
    // {
    //     if (toggleGraphic != null && toggleGraphic.Length > 0)
    //     {
    //         FadeManager.Instance.ToggleCut(toggleGraphic);
    //     }
    // }

    public void ResetTimmer()
    {
        if (coroutine == null) coroutine = StartCoroutine(ResetTimmerC());

    }

    IEnumerator ResetTimmerC()
    {

        if (isReady) yield return delay;
        answerSelector.Question();
        FadeManager.Instance.SetAlphaOne(timmerImage);
        Debug.Log($"ResetTimmer {Time.time}");

        remainTime = defaultTimmer;
        currentIndex++;
        Debug.Log($" currentIndex = {currentIndex}");


        isTimeOver = false;
        coroutine = null;
        isEvent = false;

        if (isReady == false)
        {
            isReady = true;
        }
        FadeManager.Instance.TargetFade(text, 1f);

    }

    public void IsReady()
    {
        remainTime = defaultRedayTimmer;
        isReady = false;
    }
}
