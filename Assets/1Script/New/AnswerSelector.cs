using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class AnswerSelector : MonoBehaviour
{
    [Header("질문 텍스트")]
    [SerializeField] Text[] questionTexts;
    [SerializeField] Text qText;
    [Header("대답 텍스트")]
    [SerializeField] Text[] anwserTexts;

    [SerializeField] Text aText;
    [Header("대답 텍스트 BG 할당필요")]
    [SerializeField] Image aTextImage;

    //[SerializeField] Text currentText;
    [Header("각 질문 선택한 답")]
    [SerializeField] int[] chooseNums;
    [SerializeField] int inputNum = -1;

    [Header("Q 번호")]
    public int index = 0;
    [Header("정답 나올떄 변화 x를 위해")]
    public bool isDelay = false;

    public int qSize = -1;

    void Awake()
    {
        anwserTexts = GetComponentsInChildren<Text>();

        qSize = questionTexts.Length;

        chooseNums = new int[qSize];
    }


    void Update()
    {
        if (isDelay) return;
        // 각 키를 눌렀을 때 배열의 해당 인덱스 값을 currentText에 할당합니다.
        if (Input.GetKeyDown(KeyCode.UpArrow) && anwserTexts.Length > 0)
        {
            //currentText = answerTexts[0];
            inputNum = 0;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && anwserTexts.Length > 1)
        {
            //currentText = answerTexts[1];
            inputNum = 1;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && anwserTexts.Length > 2)
        {
            //currentText = answerTexts[2];
            inputNum = 2;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && anwserTexts.Length > 3)
        {
            //currentText = answerTexts[3];
            inputNum = 3;
        }
    }

    public bool IsAnswer()
    {
        if (anwserTexts.Length > inputNum && inputNum > -1) //답변 존재
            return true;
        else return false;
    }

    public void Answer(int _index)
    {
        inputNum = _index;

        //Debug.Log($"Answer  inputNum = {inputNum}");
        FadeManager.Instance.SetAlphaOne(aText);
        FadeManager.Instance.SetAlphaOne(aTextImage);
        //Debug.Log($"anwserTexts.Length = {anwserTexts.Length}  / inputNum = {inputNum}");
        if (anwserTexts.Length > inputNum && inputNum > -1)
        {
            aText.text = anwserTexts[inputNum].text;
        }
        else
        {
            inputNum = 0;
        }

        ScoreManager.Instance.SetStep(CustomSerialController.Instance.currentButtonIndex - 1, inputNum);
        chooseNums[index] = inputNum;
        inputNum = -1;
        index++;

        // Debug.Log($" index = {index}");

    }

    public void Question()
    {
        FadeManager.Instance.SetAlphaOne(qText);
        FadeManager.Instance.SetAlphaZero(aText);
        FadeManager.Instance.SetAlphaZero(aTextImage);
        qText.text = questionTexts[index].text;
    }

    void OnEnable()
    {
        Reset();
    }



    public void Reset()
    {
        index = 0;
    }
}
