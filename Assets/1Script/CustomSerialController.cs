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

    const int BUTTONLENGTH = 14;

    const int BELLINDEX = 14;
    const int RFIDINDEX = 15;

    const int ANSWERNUM = 12;

    protected Thread[] thread = new Thread[ARDUINONUM];
    protected SerialThreadLines[] serialThread = new SerialThreadLines[ARDUINONUM];

    public Color[] arduinoState = new Color[4];

    public int[] arduinoStateInt = new int[ARDUINONUM];


    Color setcolor = new Color(0, 0, 0, 0);

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    WaitForSeconds waitSeverSend = new WaitForSeconds(0.1f);
    WaitForSeconds setcolorwait = new WaitForSeconds(0.5f);


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

    Coroutine startChoiceCoroutine = null;

    Coroutine waitAnswerCorutine = null;

    public AudioSource ledOnSource;

    public AudioSource selectLEDSource;

    Coroutine idlePageCoroutine = null;

    Coroutine allColorSetCoroutine = null;

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
        //StartCoroutine(DelayToStart(ResetLED));
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
        Debug.Log("CustomSerial  -> ResetLED");
        if (isInitialize)
        {
            for (int i = 0; i < BUTTONLENGTH; i++)
            {
                SendSerialMessage(i, "e");
                Debug.Log($"I = {i} SnedMassage = {"e"}");
            }
            SendSerialMessage(RFIDINDEX, "255,255,170");
        }



    }



    public void TagLED()
    {
        SendSerialMessage(RFIDINDEX, "138,255,18");
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

        StopAllCoroutines();
        // if (selectDelayCoroutine != null)
        // {
        //     StopCoroutine(selectDelayCoroutine);
        //     selectDelayCoroutine = null;
        // }
        // if (setColorCoroutine != null)
        // {
        //     StopCoroutine(setColorCoroutine);
        //     setColorCoroutine = null;
        // }

        // if (startChoiceCoroutine != null)
        // {
        //     StopCoroutine(startChoiceCoroutine);
        //     startChoiceCoroutine = null;
        // }

        // if (waitAnswerCorutine != null)
        // {
        //     StopCoroutine(waitAnswerCorutine);
        //     waitAnswerCorutine = null;
        // }

        // if (idlePageCoroutine != null)
        // {
        //     StopCoroutine(idlePageCoroutine);
        //     idlePageCoroutine = null;
        // }
        // if (allColorSetCoroutine != null)
        // {
        //     StopCoroutine(allColorSetCoroutine);
        //     allColorSetCoroutine = null;
        // }
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

    public Color GetColor(int index)
    {
        switch (index)
        {
            case 1: return button1;
            case 2: return button2;
            case 3: return button3;
            case 4: return button4;
            default: return defaultColor;
        }
    }



    public void WaitForSerialAnswer(int _index, Action _callbackTrigger)
    {
        if (waitAnswerCorutine != null)
        {
            StopCoroutine(waitAnswerCorutine);
            waitAnswerCorutine = null;
        }
        waitAnswerCorutine = StartCoroutine(WaitAnswerCorutine(_index, _callbackTrigger));

    }

    IEnumerator WaitAnswerCorutine(int _index, Action _callbackTrigger)
    {
        string _message = null;
        ReadSerialMessage(_index);
        yield return waitSeverSend;

        while (_message == null)
        {
            _message = ReadSerialMessage(_index);
            if (_message != null)
            {
                Debug.Log($" {_index} =  {_message}");
            }
            yield return waitSeverSend;
        }
        if (_index == 14)
        {
            yield return ButtonAllColor(defaultColor);
        }
        yield return waitSeverSend;
        _callbackTrigger?.Invoke();
    }
    public void StartChoice()
    {
        currentButtonIndex = 12;
        indexIsUP = false;
        if (waitAnswerCorutine != null)
        {
            StopCoroutine(waitAnswerCorutine);  //혹시 겹칠수있음
            waitAnswerCorutine = null;

        }
        if (startChoiceCoroutine != null)
        {
            StopCoroutine(startChoiceCoroutine);
        }
        StopAllCoroutines();

        startChoiceCoroutine = StartCoroutine(StartChoiceCoroutine());
    }

    public void BellTrigger()
    {
        StartCoroutine(ButtonAllColor(Color.black));
        //벨 버튼 LED
    }

    public void StopChoice()
    {
        if (startChoiceCoroutine != null)
        {
            StopCoroutine(startChoiceCoroutine);
            startChoiceCoroutine = null;
        }
        SendSerialMessage(13, $"{1},{defaultColor.r},{defaultColor.g},{defaultColor.b}");
    }

    IEnumerator StartChoiceCoroutine()
    {
        yield return ButtonAllColor(Color.black);
        yield return waitForFixedUpdate;
        yield return ButtonIndexColor(currentButtonIndex, defaultColor);
        SendSerialMessage(13, $"{1},{0},{0},{0}");
        yield return waitForFixedUpdate;

        while (true)
        {
            yield return waitForFixedUpdate;

            for (int i = 1; i < 13; i++)
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
    }
    public int GetAnswer()
    {
        if (selectNum == -1) return -1;
        return selectNum - 49;
    }


    public void SelectNext(int _index)
    {

        isSelect = true;
        isanswer = false;
        Debug.Log($"선택함 isSelect = true {currentButtonIndex}");
        if (selectDelayCoroutine == null)
            selectDelayCoroutine = StartCoroutine(SelectLEDDelay(_index));

    }


    IEnumerator SelectLEDDelay(int _index)
    {
        yield return wiatforSelect;

        Debug.Log($"SelectNum = {selectNum - 49}");//아스키코드값 뺴기 

        // if (currentButtonIndex == 0 || currentButtonIndex == 13)
        // {
        //     SendSerialMessage(currentButtonIndex, $"{1},{defaultColor.r},{defaultColor.g},{defaultColor.b}");
        // }

        switch (_index)
        {
            case 0:
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
                    break;
                }
            case 1:
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
                    break;
                }
            case 2:
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
                    break;
                }
            case 3:
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
                    break;
                }
        }
        selectLEDSource?.Play();
        isSelect = false;
        selectNum = -1;

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
        Debug.Log($"다음 인덱스 = {currentButtonIndex}");
        StartCoroutine(ButtonIndexColor(currentButtonIndex, defaultColor));

        for (int i = 0; i < BUTTONLENGTH; i++)
        {
            ReadSerialMessage(i);
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
        ledOnSource?.Play();
        Debug.Log($"i = {i} {message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
        yield return waitSeverSend;
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
        if (serialThread[_index] == null)
        {
            Debug.Log($"serialThread{_index}  Is Null");
            return null;
        }

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

    IEnumerator ButtonAllColor(Color _color)
    {
        for (int i = 1; i < 13; i++)
        {
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{1},{_color.r},{_color.g},{_color.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{2},{_color.r},{_color.g},{_color.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{3},{_color.r},{_color.g},{_color.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{4},{_color.r},{_color.g},{_color.b}");
        }
    }

    IEnumerator ButtonAllOriginColor()
    {
        for (int i = 1; i < 13; i++)
        {
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{1},{button1.r},{button1.g},{button1.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{2},{button2.r},{button2.g},{button2.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{3},{button3.r},{button3.g},{button3.b}");
            yield return waitForFixedUpdate;
            SendSerialMessage(i, $"{4},{button4.r},{button4.g},{button4.b}");
        }
    }
    IEnumerator ButtonIndexColor(int index, Color _color)
    {
        yield return waitForFixedUpdate;
        SendSerialMessage(index, $"{1},{_color.r},{_color.g},{_color.b}");
        yield return waitForFixedUpdate;
        SendSerialMessage(index, $"{2},{_color.r},{_color.g},{_color.b}");
        yield return waitForFixedUpdate;
        SendSerialMessage(index, $"{3},{_color.r},{_color.g},{_color.b}");
        yield return waitForFixedUpdate;
        SendSerialMessage(index, $"{4},{_color.r},{_color.g},{_color.b}");
    }
    public void StartIdlePageCoroutine()
    {
        StopAllCoroutines();
        idlePageCoroutine = StartCoroutine(StartLedLineWhiteCoroutine());
    }

    public void StopIdlePageCoroutine()
    {
        StopAllCoroutines();
        StartCoroutine(ButtonAllOriginColor());
    }

    public void SetAllwhite()
    {
        ButtonAllColor(defaultColor);
    }

    public void StartSetHintColorDelay()
    {
        StopAllCoroutines();
        StartCoroutine(SetHintColorDelay());
    }

    public void StartSetHintColor()
    {
        StopAllCoroutines();
        StartCoroutine(SetHintColor());
    }

    IEnumerator SetHintColorDelay()
    {
        yield return wait1Second;
        yield return waitForFixedUpdate;
        for (int i = 1; i < 13; i++)
        {
            setcolor = GetColor(ScoreManager.Instance.answers[i - 1] + 1);
            SendSerialMessage(i, $"{ScoreManager.Instance.answers[i - 1] + 1},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return setcolorwait;
        }
        yield return wait1Second;
        yield return wait1Second;
        yield return ButtonAllColor(defaultColor);

    }

    IEnumerator SetHintColor()
    {
        yield return waitForFixedUpdate;
        for (int i = 1; i < 13; i++)
        {
            setcolor = GetColor(ScoreManager.Instance.answers[i - 1] + 1);
            SendSerialMessage(i, $"{ScoreManager.Instance.answers[i - 1] + 1},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitForFixedUpdate;
        }
    }





    IEnumerator StartLedLineWhiteCoroutine()
    {
        int whiteIndex = 12;
        int balckIndex = 13;
        yield return waitForFixedUpdate;
        SendSerialMessage(13, $"{1},{defaultColor.r},{defaultColor.g},{defaultColor.b}");
        yield return waitForFixedUpdate;
        SendSerialMessage(15, $"{defaultColor.r},{defaultColor.g},{defaultColor.b}");
        yield return waitForFixedUpdate;
        yield return StartCoroutine(ButtonAllColor(Color.black));
        yield return waitForFixedUpdate;

        while (true)
        {
            StartCoroutine(ButtonIndexColor(whiteIndex, defaultColor));

            if (balckIndex != 13) StartCoroutine(ButtonIndexColor(balckIndex, Color.black));

            yield return setcolorwait;

            whiteIndex -= 1;
            if (whiteIndex < 1) whiteIndex = 12;
            balckIndex -= 1;
            if (balckIndex < 1) balckIndex = 12;

        }
    }

}
