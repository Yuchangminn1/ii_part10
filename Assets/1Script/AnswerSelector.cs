using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerSelector : MonoBehaviour
{
    [SerializeField] Text[] answerTexts;

    [SerializeField] Text currentText;

    [SerializeField] Text targetText;

    WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

    public int index = 0;

    void Awake()
    {
        answerTexts = GetComponentsInChildren<Text>();
    }
    void Update()
    {
        // 각 키를 눌렀을 때 배열의 해당 인덱스 값을 currentText에 할당합니다.
        if (Input.GetKeyDown(KeyCode.F1) && answerTexts.Length > 0)
        {
            currentText = answerTexts[0];
        }
        if (Input.GetKeyDown(KeyCode.F2) && answerTexts.Length > 1)
        {
            currentText = answerTexts[1];
        }
        if (Input.GetKeyDown(KeyCode.F3) && answerTexts.Length > 2)
        {
            currentText = answerTexts[2];
        }
        if (Input.GetKeyDown(KeyCode.F4) && answerTexts.Length > 3)
        {
            currentText = answerTexts[3];
        }
        if (targetText != null && currentText != null)
        {
            targetText.text = currentText.text;
        }
    }

    public void SetTargetText(Text _text)
    {
        Debug.Log("SetTargetText");
        targetText = _text;
    }

    public void Answer()
    {
        Debug.Log("Answer");
        if (index == 0)
        {
            index++;

            return;
        }
        index++;
        if (targetText == null) return;
        if (currentText == null)
            targetText.text = "테스트) F1,F2,F3,F4로 선택";
        else
            targetText.text = currentText.text;
        targetText = null;
    }

    IEnumerator DelayTargetSet(Text _text)
    {
        yield return waitForSeconds;
        Debug.Log("SetTargetText");
        targetText = _text;
    }

    public void Reset()
    {
        currentText = null;
    }
}
