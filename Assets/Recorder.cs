using UnityEngine;

public class Recorder : MonoBehaviour
{
    SequenceScript sequenceScript;
    public int[] answers = new int[12];
    int index = 0;

    void Awake()
    {
        sequenceScript = GetComponent<SequenceScript>();
    }
    void Update()
    {
        if (index < answers.Length)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                answers[index] = 0;
                index++;
                CheckEnd();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                answers[index] = 1;
                index++;
                CheckEnd();
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                answers[index] = 2;
                index++;
                CheckEnd();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                answers[index] = 3;
                index++;
                CheckEnd();
            }
        }
    }

    public void CheckEnd()
    {
        if (index == answers.Length)
        {
            ScoreManager.Instance.SetAnswer(answers);
            sequenceScript.TriggerOn();

        }

    }


}
