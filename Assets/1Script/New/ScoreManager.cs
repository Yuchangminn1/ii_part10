
using UnityEngine;



public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int[] anwsers = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] chooseStep;

    public int stamp = 0;

    public int checkAnwser = 0;


    private void Awake()
    {
        chooseStep = new int[12];
        anwsers = new int[12];
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetAnwser()
    {
        for (int i = 0; i < anwsers.Length; i++)
        {
            anwsers[i] = -1;
            checkAnwser = 0;
            chooseStep[i] = -1;
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Updaarow");
            for (int i = 0; i < chooseStep.Length; i++)
            {
                chooseStep[i] = i;
            }

            for (int i = 0; i < anwsers.Length; i++)
            {
                anwsers[i] = i;
            }
        }
    }


    public void SetStep(int _index, int _value)
    {
        chooseStep[_index] = _value + 1;
        UserDataManager.Instance.RequestUserDataUpdate(_index + 1, $"{_value}");
        Debug.Log($"chooseStep[{_index}] = {_value}; ");
    }

    public int GetLastStep()
    {
        return chooseStep[chooseStep.Length - 1];
    }

    public int Checker()
    {
        int hitNum = 0;
        for (int i = 0; i < anwsers.Length; i++)
        {
            if (chooseStep[i] == anwsers[i])
            {
                hitNum++;
                Debug.Log($"i ={i} cho{chooseStep[i]} ==  anw{anwsers[i]}");

            }
            else
            {
                Debug.Log($"i ={i} cho{chooseStep[i]} /  anw{anwsers[i]}");
            }
        }
        UserDataManager.Instance.RequestUserContentEnd();


        return hitNum;
    }
    public void SetAnswer(int _index, int value)
    {

        if (_index < 0 || _index >= anwsers.Length)
        {
            Debug.Log("SetAnswer Scoremanager 잘못된 인덱스 값");
            return;
        }
        else
        {
            Debug.Log($"Score Manager anwser{_index} = {value}");
        }
        anwsers[_index] = value;
        Debuger();

    }

    public int Debuger()
    {
        int hitNum = 0;
        for (int i = 0; i < anwsers.Length; i++)
        {
            if (anwsers[i] != -1)
            {
                Debug.Log($"Answer{i} = {anwsers[i]}");
            }
        }
        return hitNum;
    }





}
