using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : SequenceScript
{
    [SerializeField] private Text targetText; // 타이핑 효과를 적용할 UI Text
    [SerializeField] private float letterDelay = 0.2f; // 한 글자씩 출력되는 간격


    string originalText;

    WaitForSeconds _letterDelay;


    [SerializeField] bool hasToggle = false;

    Color color = new Color(1, 1, 1, 0);

    protected override void AwakeSetup()
    {
        if (targetText == null) targetText = GetComponent<Text>();
        _letterDelay = new WaitForSeconds(letterDelay);
        originalText = targetText.text;
        if (startDelay > 0f && _startDelay == null) _startDelay = new WaitForSeconds(startDelay);
    }

    private void OnEnable()
    {
        if (targetText != null) targetText.color = color;
        if (hasToggle)
        {
            ToggleText();
        }
    }

    public void ToggleText()
    {
        targetText.enabled = !targetText.enabled;
        hasToggle = !hasToggle;
    }

    protected override IEnumerator RunSequence()
    {
        if (targetText.enabled)
        {

            targetText.text = "";
            targetText.color = Color.black;
            if (_startDelay != null) yield return _startDelay;

            foreach (char letter in originalText)
            {
                targetText.text += letter; // 한 글자씩 추가
                yield return _letterDelay;
            }
        }

        yield return null;
    }

    public void StartTyping(string fullText)
    {
        // 코루틴 시작 전에 텍스트를 초기화
        targetText.text = "";
        targetText.color = Color.black;
        StartCoroutine(TypeTextCoroutine(fullText));
    }

    private IEnumerator TypeTextCoroutine(string fullText)
    {
        if (_startDelay != null) yield return _startDelay;

        foreach (char letter in fullText)
        {
            targetText.text += letter; // 한 글자씩 추가
            yield return _letterDelay;
        }
    }
}