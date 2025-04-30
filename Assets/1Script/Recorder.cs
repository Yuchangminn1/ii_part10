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



}
