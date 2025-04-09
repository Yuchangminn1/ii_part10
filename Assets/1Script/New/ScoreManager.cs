
using UnityEngine;



public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int[] answers;

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

    public void SetAnswer(int[] _answers)
    {
        answers = _answers;
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





}
