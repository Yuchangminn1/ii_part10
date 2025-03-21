using System;
using UnityEngine;
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

    Color colorB;

    bool isTimeOver;



    public int currentIndex = 1;

    void Awake()
    {
        text = GetComponent<Text>();
        pageSequenceController = GetComponentInParent<PageSequenceController>();

    }

    void OnEnable()
    {
        currentIndex = 1;
        text.text = "";
        isTimeOver = false;

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

        }
        text.text = $"{(int)Math.Ceiling(remainTime)}";
    }

    public void ResetTimmer()
    {

        pageSequenceController?.CurrentIndexTriggerON();

        remainTime = defaultTimmer;
        currentIndex += 2;

        colorB = text.color;
        colorB.a = 1f;
        text.color = colorB;
        isTimeOver = false;

    }
}
