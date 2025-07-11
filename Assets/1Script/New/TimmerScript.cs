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

    float popup1timmer1 = 10f;
    float popup1timmer2 = 10f;


    [SerializeField] float remainTime = 0f;

    public float timeSpeed = 1f;

    [SerializeField] UnityEvent time3SecEvent;

    [SerializeField] UnityEvent endEvent;


    [Header("해당 페이지 내 존재하는 스크립트 할당 해야함")]
    [SerializeField] AnswerSelector answerSelector;


    bool isEvent;

    bool isTimeOver;

    WaitForSeconds delay = new WaitForSeconds(1f);

    Coroutine coroutine;

    public int currentIndex = 1;

    public bool isReady = false;

    public Image timmerImage;

    public float popupTime = 15f;
    public float defaultPopupTime = 3f;

    public Coroutine delayCoroutine = null;

    public bool isEnd = false;

    AudioSource downCountSource;

    public UnityEvent onCountStart;

    int popupCount = 0;



    void Awake()
    {
        text = GetComponent<Text>();
        popupTime = popup1timmer1;
    }
    void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }

    public void SetDefaultTime(float _time)
    {
        defaultTimmer = _time;
        Debug.Log(" time = " + _time);
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

        remainTime -= Time.deltaTime * timeSpeed;
        if (remainTime < 0f && coroutine == null)
        {

            remainTime = 0f;
            downCountSource?.Stop();

            isTimeOver = true;
            text.text = "0";

            if (isReady == false) // 초반 3초 
            {
                FadeManager.Instance.SetAlphaZero(isReadyText);

                text.text = $"{(int)Math.Ceiling(remainTime)}";
                FadeManager.Instance.TargetFade(text, 0f);

                if (answerSelector.qSize > currentIndex)
                {
                    ResetTimmer();
                }
                CustomSerialController.Instance.StartChoice();
                return;
            }

            CheckAnswer();
            return;

        }
        if (!isEvent && remainTime < 6f && remainTime > 4f)
        {
            isEvent = true;
            time3SecEvent?.Invoke();
        }
        text.text = $"{(int)Math.Ceiling(remainTime)}";
    }

    IEnumerator Delay(float _time, Action _callback)
    {
        yield return new WaitForSeconds(_time);
        _callback?.Invoke();
        delayCoroutine = null;
    }

    public void CheckAnswer()
    {
        if (isEnd) return;
        if (delayCoroutine != null) return;
        int _answerNum = CustomSerialController.Instance.GetAnswer();
        if (_answerNum == -1) // 대답 못했을 때 팝업창
        {
            PopupCheck();

            return;
        }
        text.text = "";
        PopupManager.Instance.ResetIndex();
        FadeManager.Instance.SetAlphaZero(text);
        FadeManager.Instance.SetAlphaZero(timmerImage);
        CustomSerialController.Instance.SelectNext(_answerNum);
        answerSelector.Answer(_answerNum);
        ScoreManager.Instance.SetAnswer(CustomSerialController.Instance.currentButtonIndex - 1, _answerNum + 1);
        //ScoreManager.Instance.answers[CustomSerialController.Instance.currentButtonIndex - 1] = _answerNum;
        if (answerSelector.qSize <= currentIndex)
        {
            isEnd = true;
            Debug.Log("endEvent");
            endEvent?.Invoke();
        }
        else
        {
            ResetTimmer();

        }

    }

    private void PopupCheck()
    {
        if (popupCount == 0)
        {
            popupTime = popup1timmer1;
            PopupManager.Instance.Popup(0);

            popupCount = 1;
        }
        if (popupCount == 1 && popupTime < 0f)
        {
            popupCount = 2;
            PopupManager.Instance.Popup(0);


            if (delayCoroutine == null) delayCoroutine = StartCoroutine(Delay(defaultPopupTime, () => PageController.Instance.CurrentPage = 0));
            popupTime = defaultPopupTime;
        }

        popupTime -= Time.deltaTime;
    }

    public void ResetTimmer()
    {
        if (coroutine == null) coroutine = StartCoroutine(ResetTimmerC());
    }

    IEnumerator ResetTimmerC()
    {
        popupTime = defaultPopupTime;
        PopupManager.Instance.ResetIndex();
        if (isReady) yield return delay;
        answerSelector.Question();
        popupCount = 0;
        FadeManager.Instance.SetAlphaOne(timmerImage);

        remainTime = defaultTimmer;
        downCountSource?.PlayOneShot(downCountSource.clip, 5f);

        currentIndex++;

        isTimeOver = false;
        coroutine = null;
        isEvent = false;

        isEnd = false;

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
