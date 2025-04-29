using UnityEngine;
using System.Threading;
using System.Collections;
using System;
using TMPro;

public class CustomSerialController : MonoBehaviour
{

    private static CustomSerialController instance;

    public static CustomSerialController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CustomSerialController>();
                if (instance == null)
                {
                    Debug.Log("CustomSerialController Is Null");
                }
            }

            return instance;
        }
    }
    // Helper to send color with proper RGB order and integer conversion
    private void SendRgb(int index, Color color)
    {
        int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255);
        int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255);
        int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255);
        // Append newline to clearly delimit messages, include index as first value
        SendSerialMessage(index, $"{index},{r},{g},{b}\n");
    }
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string[] portName;

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 9600;

    [Tooltip("Reference to an scene object that will receive the events of connection, " +
             "disconnection and the messages from the serial device.")]
    public GameObject messageListener;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 1;

    public AudioSource errorSound;

    public AudioSource tagsound;

    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

    // Internal reference to the Thread and the object that runs in it.
    const int ARDUINONUM = 16;

    const int ANSWERNUM = 12;

    protected Thread[] thread = new Thread[ARDUINONUM];
    protected SerialThreadLines[] serialThread = new SerialThreadLines[ARDUINONUM];

    public Color[] arduinoState = new Color[4];

    public int[] arduinoStateInt = new int[ARDUINONUM];


    Color setcolor = new Color(0, 0, 0, 0);

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    WaitForSeconds waitSeverSend = new WaitForSeconds(0.1f);
    WaitForSeconds setcolorwait = new WaitForSeconds(0.3f);


    WaitForSeconds wait1Second = new WaitForSeconds(1f);
    WaitForSeconds wiatforSelect = new WaitForSeconds(3f);

    WaitForSeconds topLedResetWait = new WaitForSeconds(60f);

    //Color[] testcolor = new Color[3];

    Coroutine selectDelayCoroutine = null;

    Coroutine setColorCoroutine = null;

    Color defaultColor = new Color(255, 255, 170);

    public int currentButtonIndex = 0;

    public bool isSelect = true;

    public bool indexIsUP = false;


    Color button1 = new Color(220, 210, 5);
    Color button2 = new Color(245, 50, 100);
    Color button3 = new Color(40, 250, 255);
    Color button4 = new Color(50, 205, 120);

    Color errorColor = new Color(230, 0, 5);

    public bool isInitialize = false;

    public bool isInitialize22 = false;

    public bool isanswer = false;

    float timmer = 10f;

    int selectNum = -1;




    enum PortState
    {
        OFF = 0,        // color off
        ON = 1,         // color white
        SELECT = 2,    // color yellow
        ERROR = 3,      // color red
    };

    void Awake()
    {

    }


    public void Initialize(string[] portNames)
    {
        Debug.Log($"포트 초기화: {string.Join(", ", portNames)}");

        int portCount = portNames.Length;

        Debug.Log($"포트 수 {portCount}");

        for (int i = 0; i < portCount; i++)
        {
            serialThread[i] = new SerialThreadLines(portNames[i], baudRate, reconnectionDelay, maxUnreadMessages);
            thread[i] = new Thread(new ThreadStart(serialThread[i].RunForever));
            thread[i].Start();
            Debug.Log($"쓰레드시작 {i}");
        }
        Debug.Log($"serialThread = {serialThread.Length}");
        isInitialize = true;
        StartCoroutine(DelayToStart(ResetLED));
    }

    public void SetColor(Color[] _color)
    {
        // 전달받은 Color 배열을 내부 상태에 저장
        arduinoState = _color;

        // 각 Color의 채널을 0~255 정수값으로 변환하여 로그 메시지 구성
        string logMessage = "SetColor = ";
        foreach (Color col in _color)
        {
            int r = Mathf.RoundToInt(col.r * 255);
            int g = Mathf.RoundToInt(col.g * 255);
            int b = Mathf.RoundToInt(col.b * 255);
            int a = Mathf.RoundToInt(col.a * 255);
            logMessage += $"RGBA({r}, {g}, {b}, {a}) ";
        }
        Debug.Log(logMessage);
    }

    public void ResetLED()
    {
        for (int i = 0; i < serialThread.Length; i++)
        {
            SendSerialMessage(i, "e");
            Debug.Log($"I = {i} SnedMassage = {"e"}");
        }
    }

    IEnumerator DelayToStart(Action _callback)
    {

        yield return new WaitForSeconds(5f);
        _callback?.Invoke();
        yield return new WaitForSeconds(5f);
        isInitialize22 = true;


    }

    // 시리얼 연결/해제 이벤트도 받을 수 있습니다.

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void OnEnable()
    {

    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    void OnDisable()
    {

        if (userDefinedTearDownFunction != null)
            userDefinedTearDownFunction();

        if (serialThread != null)
        {
            for (int i = 0; i < serialThread.Length; i++)
            {
                serialThread[i].RequestStop();
            }
            serialThread = null;
        }
        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            for (int i = 0; i < thread.Length; i++)
            {
                thread[i].Join();
            }
            thread = null;
        }

        if (selectDelayCoroutine != null)
        {
            StopCoroutine(selectDelayCoroutine);
            selectDelayCoroutine = null;
        }
        if (setColorCoroutine != null)
        {
            StopCoroutine(setColorCoroutine);
            setColorCoroutine = null;
        }


    }
    public Color GetColor(char index)
    {
        switch (index)
        {
            case '1': return button1;
            case '2': return button2;
            case '3': return button3;
            case '4': return button4;
            default: return defaultColor;
        }
    }

    void FixedUpdate()
    {
        if (timmer > 0)
        {
            timmer -= Time.deltaTime;
        }
        if (timmer < 0)
        {
            if (isanswer)
            {
                isSelect = true;
                isanswer = false;
                Debug.Log($"선택함 isSelect = true {currentButtonIndex}");
                if (selectDelayCoroutine == null)
                    selectDelayCoroutine = StartCoroutine(SelectLEDDelay(false));
            }
            else
            {
                Debug.Log($"{currentButtonIndex} IS Answer = false");
            }

        }


        for (int i = 0; i < serialThread.Length; i++)
        {
            string message = ReadSerialMessage(i);
            if (message != null)
            {
                Debug.Log($" {i} =  {message}");

                if (i == currentButtonIndex)
                {
                    SetButtonLED(currentButtonIndex, message);
                }

            }


        }

    }

    IEnumerator SelectLEDDelay(bool _tf)
    {
        yield return wiatforSelect;

        Debug.Log($"SelectNum = {selectNum - 49}");//아스키코드값 뺴기 

        if (selectNum - 49 == 0)
        {
            setcolor = GetColor('1');
            SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
            setcolor = Color.black;
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;
            SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");

        }
        else if (selectNum - 49 == 1)
        {
            setcolor = GetColor('2');

            SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
            setcolor = Color.black;

            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");
        }
        else if (selectNum - 49 == 2)
        {
            setcolor = GetColor('3');

            SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");

            setcolor = Color.black;
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");
        }
        else if (selectNum - 49 == 3)
        {
            setcolor = GetColor('4');

            SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");

            setcolor = Color.black;
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitSeverSend;

            SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
        }

        isSelect = _tf;

        if (!_tf)
        {
            Debug.Log($"isSelect = false");

            if (indexIsUP)
            {
                if (currentButtonIndex + 1 > 13)
                {
                    Debug.Log("currentButtonIndex = " + currentButtonIndex);
                    indexIsUP = false;
                }
                else
                {
                    currentButtonIndex += 1;
                    Debug.Log("currentButtonIndex = " + currentButtonIndex);

                }
            }
            else
            {
                if (currentButtonIndex - 1 < 0)
                {
                    indexIsUP = true;
                    Debug.Log("currentButtonIndex = " + currentButtonIndex);

                }
                else
                {
                    currentButtonIndex -= 1;
                    Debug.Log("currentButtonIndex = " + currentButtonIndex);


                }
            }
        }



        timmer = 10f;

        selectDelayCoroutine = null;

    }

    private void SetButtonLED(int i, string _message)
    {
        // 1. 메시지가 null이거나 빈 문자열이면 아무 처리도 하지 않고 반환

        // 2. 이미 선택 상태면 반환
        if (isSelect)
        {
            Debug.Log("IsSelect IS True");
            return;
        }

        // 4. 물음표 확인
        if (_message[0] == '?')
        {
            Debug.Log("Message = ?");
            return;
        }

        // 5. 두 자리 인덱스 처리 (i > 9)
        if (i > -1 && i < 14)
        {
            // 5-1. 네 글자 이상인지 확인

            if (_message[2] == 'N')
            {

                if (setColorCoroutine == null)
                    setColorCoroutine = StartCoroutine(SetColorCoroutine(i, _message));
            }
            else
            {
                setcolor = GetColor('5'); // defaultColor 유도
                SendSerialMessage(i, $"{_message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
                Debug.Log($"i = {i} {_message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
            }
        }



    }

    private IEnumerator SetColorCoroutine(int i, string message)
    {
        setcolor = GetColor(message[0]);
        SendSerialMessage(i, $"{message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
        Debug.Log($"i = {i} {message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
        yield return setcolorwait;
        isanswer = true;
        selectNum = message[0];
        Debug.Log($"SelectNum = {selectNum}");
        setColorCoroutine = null;
    }



    // ------------------------------------------------------------------------
    // Returns a new unread message from the serial device. You only need to
    // call this if you don't provide a message listener.
    // ------------------------------------------------------------------------
    public string ReadSerialMessage(int _index)
    {
        // Read the next message from the queue

        return (string)serialThread[_index].ReadMessage();
    }

    // ------------------------------------------------------------------------
    // Puts a message in the outgoing queue. The thread object will send the
    // message to the serial device when it considers it's appropriate.
    // ------------------------------------------------------------------------
    public void SendSerialMessage(int _index, string message)
    {
        if (serialThread[_index] == null)
            return;
        serialThread[_index].SendMessage(message);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function before Unity closes the COM port, so
    // the user can send some tear-down message to the hardware reliably.
    // ------------------------------------------------------------------------
    public delegate void TearDownFunction();
    private TearDownFunction userDefinedTearDownFunction;
    public void SetTearDownFunction(TearDownFunction userFunction)
    {
        this.userDefinedTearDownFunction = userFunction;
    }

}
