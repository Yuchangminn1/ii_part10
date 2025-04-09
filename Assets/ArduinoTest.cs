using UnityEngine;

public class ArduinoTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] SerialController serialController;

    float time = 0f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 2f)
        {
            time = 0f;
            serialController.SendSerialMessage("Hello Arduino!");
        }
    }
}
