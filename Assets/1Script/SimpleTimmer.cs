using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTimmer : MonoBehaviour
{
    Text text;

    Graphic graphic;

    float defaultTime = 10f;

    float time = 0;

    bool isEnd = false;

    float timeSpeed = 10f;


    private void Awake()
    {
        text = GetComponent<Text>();
        graphic = GetComponent<Graphic>();
    }

    void OnEnable()
    {
        isEnd = false;
        if (graphic != null) FadeManager.Instance.SetAlphaZero(graphic);

    }


    void FixedUpdate()
    {
        if (isEnd) return;

        time -= Time.deltaTime * timeSpeed;
        if (time < 0f)
        {
            isEnd = true;
            if (graphic != null) FadeManager.Instance.SetAlphaZero(graphic);

        }
        text.text = $"{(int)Math.Ceiling(time)}";
    }

    public void ResetTimmer()
    {
        time = defaultTime;
        isEnd = false;
        if (graphic != null) FadeManager.Instance.SetAlphaOne(graphic);
    }

}
