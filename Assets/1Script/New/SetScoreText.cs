using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SetScoreText : MonoBehaviour
{
    Text text;
    public string combinedText;

    string st1 = "12개 중에\n";
    string stw = "개를 맞췄어요!";


    string originText = "";

    Coroutine coroutine = null;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void OnEnable()
    {
        if (text != null && CustomSerialController.Instance != null)
        {
            text.text = st1 + CustomSerialController.Instance.checkNum + stw;
            Debug.Log(" CustomSerialController.Instance.checkNum");
            CustomSerialController.Instance.checkNum = 0;
        }
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
    IEnumerator DealySetText()
    {
        while (text.text == "")
        {
            yield return new WaitForSeconds(0.1f);
        }
        SetText();
    }


    public void SetText()
    {
        if (text.text != "")
        {
            if (originText == "")
                originText = text.text;
            CombineTextParts(originText);
        }

    }
    void CombineTextParts(string _inputText)
    {
        // '\n' 기준으로 문자열 분리 (빈 항목은 무시)
        string[] parts = _inputText.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        combinedText = parts[0] + $"{CustomSerialController.Instance.checkNum}" + parts[1];

        // // 분리된 결과가 두 개 이상이라면 앞의 두 문자열을 연결합니다.
        // if (parts.Length >= 2)
        // {
        //     // 두 문자열 사이에 공백이나 원하는 구분자를 추가할 수 있습니다.
        //     if (text != null && ScoreManager.Instance)
        //         combinedText = parts[0] + $"{ScoreManager.Instance.Checker()}" + parts[1];
        // }
        // else if (parts.Length == 1)
        // {
        //     // 하나의 문자열만 있다면 그대로 저장
        //     combinedText = parts[0];
        // }
        // else
        // {
        //     combinedText = "";
        // }

        Debug.Log("합쳐진 문자열: " + combinedText);
        text.text = combinedText;

    }
}


