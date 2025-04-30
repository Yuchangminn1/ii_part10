
using UnityEngine;



public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int[] answers = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] chooseStep;

    public int stamp = 0;

    private void Awake()
    {
        chooseStep = new int[12];
        answers = new int[12];
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void SetStep(int _index, int _value)
    {
        chooseStep[_index] = _value;
        Debug.Log($"chooseStep[{_index}] = {_value}; ");
    }

    public int GetLastStep()
    {
        return chooseStep[chooseStep.Length - 1];
    }

    public int Checker()
    {
        int hitNum = 0;
        for (int i = 0; i < answers.Length; i++)
        {
            if (chooseStep[i] == answers[i])
            {
                hitNum++;
            }
        }
        return hitNum;
    }
    public void SetAnswer(int _index, int value)
    {
        if (_index < 0 || _index >= answers.Length)
        {
            Debug.Log("SetAnswer Scoremanager 잘못된 인덱스 값");
            return;
        }
        else
        {
            Debug.Log($"Score Manager anwser{_index} = {value}");
        }
        answers[_index] = value;
        Debuger();
    }

    public int Debuger()
    {
        int hitNum = 0;
        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i] != -1)
            {
                Debug.Log($"Answer{i} = {answers[i]}");
            }
        }
        return hitNum;
    }





}
