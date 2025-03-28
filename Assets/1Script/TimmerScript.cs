using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimmerScript : MonoBehaviour
{
    //배속추가 
    Text text;

    [SerializeField] float defaultTimmer = 10f;
    [SerializeField] float remainTime = 0f;

    public float timeSpeed = 10f;

    //[SerializeField] bool isEnd = false;

    [SerializeField] PageSequenceController pageSequenceController;

    [SerializeField] UnityEvent time5SecEvent;
    [SerializeField] UnityEvent timmerStartEvent;
    [SerializeField] UnityEvent timmerEndEvent;


    bool isEvent;

    Color colorB;

    bool isTimeOver;

    WaitForSeconds corDelay = new WaitForSeconds(0.15f);

    Coroutine coroutine;

    public int currentIndex = 1;

    void Awake()
    {
        text = GetComponent<Text>();
        pageSequenceController = GetComponentInParent<PageSequenceController>();

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
        currentIndex = 1;
        text.text = "";
        isTimeOver = false;
        ResetTimmer();

    }

    void FixedUpdate()
    {
        if (isTimeOver) return;
        remainTime -= Time.deltaTime * timeSpeed;
        if (remainTime < 0f)
        {
            pageSequenceController?.CurrentIndexTriggerON();

            remainTime = 0f;
            text.text = "";
            colorB = text.color;
            colorB.a = 0f;
            text.color = colorB;
            isTimeOver = true;
            if (timmerEndEvent != null)
                timmerEndEvent?.Invoke();


        }
        if (!isEvent && remainTime < 6f && remainTime > 4f)
        {
            isEvent = true;
            time5SecEvent?.Invoke();
        }
        text.text = $"{(int)Math.Ceiling(remainTime)}";
    }

    IEnumerator ResetTimmerC()
    {
        yield return corDelay;
        Debug.Log($"ResetTimmer {Time.time}");

        pageSequenceController?.CurrentIndexTriggerON();

        remainTime = defaultTimmer;
        currentIndex = pageSequenceController.CurrentIndex;

        colorB = text.color;
        colorB.a = 1f;
        text.color = colorB;
        isTimeOver = false;
        coroutine = null;
        isEvent = false;
        if (timmerStartEvent != null)

            timmerStartEvent?.Invoke();
    }

    public void ResetTimmer()
    {
        if (coroutine == null) coroutine = StartCoroutine(ResetTimmerC());

    }
}
