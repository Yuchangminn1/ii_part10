
using UnityEngine;

[System.Serializable]
public struct Step
{
    public bool a;
    public bool b;
    public bool c;
    public bool d;

    public Step(bool a, bool b, bool c, bool d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }

    public static Step CreateWithIndex(int index)
    {
        return new Step(
            a: index == 0,
            b: index == 1,
            c: index == 2,
            d: index == 3
        );
    }
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    Step[] steps;

    bool[] answers = new bool[12];

    public int[] chooseStep = new int[12];

    public int stamp = 0;

    private void Awake()
    {
        steps = new Step[12];
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialized()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i] = new Step(false, false, false, false);
        }

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


    public int GetNumberOfCorrects()
    {
        int numberOfCorrects = 0;
        foreach (bool answer in answers)
        {
            if (answer) numberOfCorrects++;
        }
        return numberOfCorrects;
    }


}
