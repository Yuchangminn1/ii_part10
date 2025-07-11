﻿using UnityEngine;
using System.Threading;
using System.Collections;
using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.UI;

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

    //public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    //public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

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
    WaitForSeconds wiatforSelect = new WaitForSeconds(1f);

    Coroutine selectDelayCoroutine = null;

    Coroutine setColorCoroutine = null;
    Coroutine checkColorCoroutine = null;

    public int userAnswerNum = 0;


    Color defaultColor = new Color(255, 255, 170);

    public int currentButtonIndex = 0;

    public bool isSelect = true;

    public bool indexIsUP = false;

    public GameObject wallVideoGameObject;


    Color button1 = new Color(220, 210, 5);
    Color button2 = new Color(40, 250, 255);

    Color button3 = new Color(245, 50, 100);
    Color button4 = new Color(50, 205, 120);

    Color errorColor = new Color(230, 0, 5);

    public SubVideoPlayer hintText;



    public bool isInitialize = false;

    public bool isanswer = false;

    float timmer = 10f;

    int selectNum = -1;

    Coroutine startChoiceCoroutine = null;
    Coroutine startReturnChoice = null;

    Coroutine waitAnswerCorutine = null;

    public AudioSource ledOnSource;

    public AudioSource selectLEDSource;

    Coroutine idlePageCoroutine = null;

    public UnityEvent userMissButtonEvent;

    public bool isDelayAppliedWhenWrong = false;

    public AudioSource longSound;

    public UnityEvent errorAnswer;

    public int checkNum = 0;

    public bool iswait = false;

    public int[] dap = new int[12];

    public GameObject[] VideoObject = new GameObject[2];
    public VideoPlayer[] VideoPlayer = new VideoPlayer[2];
    public Graphic[] VideoGraphic = new Graphic[2];

    public Coroutine[] coroutineObject = new Coroutine[2];

    public int debugint = -1;

    public void SetObject()
    {
        for (int i = 0; i < VideoObject.Length; i++)
        {
            VideoPlayer[i] = VideoObject[i].GetComponent<VideoPlayer>();
            VideoGraphic[i] = VideoObject[i].GetComponent<Graphic>();
        }
        coroutineObject = new Coroutine[(VideoObject.Length)];
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
        SetObject();


        //userMissButtonEvent.AddListener(ErrorEvent);
    }
    public void ErrorEvent()
    {
        SetObjectIndex(0);
        SetObjectIndex(1);
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

    public void SetObjectIndex(int _index)
    {
        if (VideoObject.Length < _index)
        {
            if (coroutineObject[_index] != null)
            {
                StopCoroutine(coroutineObject[_index]);
                coroutineObject[_index] = null;
            }
            coroutineObject[_index] = StartCoroutine(SetObjectCoroutine(_index));
        }
    }

    IEnumerator SetObjectCoroutine(int _index)
    {
        if (VideoObject[_index] != null)
        {
            VideoObject[_index].SetActive(true);
            FadeManager.Instance.SetAlphaOne(VideoGraphic[_index]);
            yield return new WaitForSeconds(0.1f);
            while (VideoPlayer[_index].isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }
            FadeManager.Instance.SetAlphaZero(VideoGraphic[_index]);
            yield return new WaitForSeconds(0.1f);
            VideoObject[_index].SetActive(false);
        }
    }


    public void TagLED()
    {
        SendSerialMessage(RFIDINDEX, "138,255,18");
    }

    void OnDisable()
    {
        for (int i = 0; i < VideoObject.Length; i++)
        {
            if (coroutineObject[i] != null)
            {
                StopCoroutine(coroutineObject[i]);
                coroutineObject[i] = null;
            }
        }


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

    public void CheckNumReset()
    {
        checkNum = 0;
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
        userAnswerNum = 0;
        currentButtonIndex = 12;
        indexIsUP = false;

        StopAllCoroutines();

        startChoiceCoroutine = StartCoroutine(StartChoiceCoroutine());

    }

    IEnumerator StartChoiceCoroutine()
    {
        ScoreManager.Instance.ResetAnwser();
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
                if (message != null || debugint != -1)
                {
                    if (debugint != -1)
                    {
                        SetButtonLED(currentButtonIndex, $"debugint");
                    }

                    Debug.Log($" {i} =  {message}");

                    if (i == currentButtonIndex)
                    {
                        SetButtonLED(currentButtonIndex, message);
                    }
                }
            }
        }
    }
    //이어하기
    public void RStartReturnChoice()
    {
        indexIsUP = true;
        StopAllCoroutines();

        startReturnChoice = StartCoroutine(StartReturnChoiceCoroutine());
    }

    public void StartReturnChoice()
    {
        currentButtonIndex = 1;
        indexIsUP = true;
        StopAllCoroutines();

        startReturnChoice = StartCoroutine(StartReturnChoiceCoroutine());
    }

    IEnumerator StartReturnChoiceCoroutine()
    {
        Debug.Log("StartReturnChoice");
        yield return waitForFixedUpdate;
        while (currentButtonIndex < 13)
        {
            yield return waitForFixedUpdate;

            if (isDelayAppliedWhenWrong == false)
            {
                if (iswait)
                {

                }
                else
                {
                    for (int i = 1; i < 13; i++)
                    {
                        string message = ReadSerialMessage(i);
                        if (message != null)
                        {
                            Debug.Log($" {i} =  {message}");

                            if (i == currentButtonIndex)
                            {
                                SetCheckErrorLED(currentButtonIndex, message);
                            }
                        }
                    }
                }


            }

        }
        PageController.Instance.NextButton();
    }


    private void SetCheckErrorLED(int i, string _message)
    {
        Debug.Log("SetCheckErrorLED");
        if (i > -1 && i < 14)
        {
            // 5-1. 네 글자 이상인지 확인
            if (_message[2] == 'N')
            {
                if (checkColorCoroutine != null)
                {
                    StopCoroutine(checkColorCoroutine);
                    checkColorCoroutine = null;
                }
                checkColorCoroutine = StartCoroutine(SetCheckColorCoroutine(i, _message));
            }
        }
    }

    public void ChooseDebug1()
    {
        selectNum = 1;
    }

    public void AnswerDebug1()
    {
        Debug.Log("ddd");
        StartCoroutine(SetCheckColorCoroutine(currentButtonIndex, "1"));
    }
    public void AnswerDebug2()
    {
        Debug.Log("ddd");
        StartCoroutine(SetCheckColorCoroutine(currentButtonIndex, "2"));

    }

    private IEnumerator SetCheckColorCoroutine(int i, string message)
    {
        char anwsers = ScoreManager.Instance.anwsers[i - 1].ToString()[0];
        if (anwsers == message[0])
        {
            Debug.Log("정답");
            checkNum += 1;
            userAnswerNum++;
            ScoreManager.Instance.checkAnwser++;
            setcolor = GetColor(message[0]);
            SendSerialMessage(i, $"{message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
            ledOnSource?.Play();
            Debug.Log($"i = {i} {message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
            currentButtonIndex++;
        }
        else
        {
            Debug.Log("오답");
            iswait = true;

            setcolor = errorColor;
            hintText?.StartSeq();
            wallVideoGameObject.SetActive(true);
            longSound?.PlayOneShot(longSound.clip, 40f);
            SendSerialMessage(i, $"{message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
            StartSetHintColor();
            Debug.Log($"i = {i} {message[0]},{setcolor.r},{setcolor.g},{setcolor.b}");
            //isDelayAppliedWhenWrong = true;
            yield return new WaitForSeconds(4f);
            for (int j = currentButtonIndex + 1; j < 13; j++)
            {
                ReadSerialMessage(j);
                yield return ButtonIndexColor(j, defaultColor);
            }
            yield return waitForFixedUpdate;
            iswait = false;

            currentButtonIndex++;

            //RStartReturnChoice();

        }


        yield return waitSeverSend;
        setColorCoroutine = null;
    }

    public void BellTrigger()
    {
        StartCoroutine(ButtonAllColor(Color.black));
        SendSerialMessage(14, "1,250,250,170");
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

    /// <summary>
    /// 한줄씩 발판 선택하는 코루틴
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    IEnumerator SelectLEDDelay(int _index)
    {
        yield return wiatforSelect;

        Debug.Log($"SelectNum = {selectNum - 49}");//아스키코드값 뺴기 

        switch (_index)
        {
            case 0:
                {
                    setcolor = GetColor('1');
                    dap[currentButtonIndex - 1] = 1;

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
                    dap[currentButtonIndex - 1] = 2;

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
                    dap[currentButtonIndex - 1] = 3;

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
                    dap[currentButtonIndex - 1] = 4;

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
    //갈때 선택하는 디버그 코드 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ChooseDebug1();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AnswerDebug1();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            AnswerDebug2();
        }
    }


    private IEnumerator SetColorCoroutine(int i, string message)
    {
        setcolor = GetColor(message[0]);
        switch (message[0])
        {
            case '1':
                {
                    setcolor = GetColor('1');
                    dap[currentButtonIndex - 1] = 1;

                    SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
                    setcolor = defaultColor;

                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;
                    SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");
                    break;
                }
            case '2':
                {
                    setcolor = GetColor('2');
                    dap[currentButtonIndex - 1] = 2;

                    SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");

                    setcolor = defaultColor;

                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");

                    yield return waitSeverSend;
                    SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");
                    break;
                }
            case '3':
                {
                    setcolor = GetColor('3');
                    dap[currentButtonIndex - 1] = 3;

                    SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");

                    setcolor = defaultColor;
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");
                    break;
                }
            case '4':
                {
                    setcolor = GetColor('4');
                    dap[currentButtonIndex - 1] = 4;

                    SendSerialMessage(currentButtonIndex, $"{4},{setcolor.r},{setcolor.g},{setcolor.b}");

                    setcolor = defaultColor;

                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{1},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{2},{setcolor.r},{setcolor.g},{setcolor.b}");
                    yield return waitSeverSend;

                    SendSerialMessage(currentButtonIndex, $"{3},{setcolor.r},{setcolor.g},{setcolor.b}");
                    break;
                }
        }
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
            yield return ButtonIndexColor(i, _color);
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
    /// <summary>
    /// 제거 1순위
    /// </summary>
    public void StartIdlePageCoroutine()
    {
        StopAllCoroutines();
        idlePageCoroutine = StartCoroutine(StartLedLineWhiteCoroutine());
    }
    /// <summary>
    /// 각 발판 행에 맞는 색으로 
    /// </summary>
    public void StopIdlePageCoroutine()
    {
        StopAllCoroutines();
        StartCoroutine(ButtonAllOriginColor());
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

    public void SetAllwhite()
    {
        ButtonAllColor(defaultColor);
    }
    /// <summary>
    /// 종 누른 후 뒤 돌았을 떄 보이는 흰트들
    /// </summary>
    /// <returns></returns>
    public void StartSetHintColorDelay()
    {
        StartCoroutine(SetHintColorDelay());
    }

    IEnumerator SetHintColorDelay()
    {
        yield return wait1Second;
        yield return waitForFixedUpdate;
        for (int i = 1; i < 13; i++)
        {
            setcolor = GetColor(ScoreManager.Instance.anwsers[i - 1]);
            SendSerialMessage(i, $"{ScoreManager.Instance.anwsers[i - 1]},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return setcolorwait;
        }
        yield return new WaitForSeconds(7f);

        yield return ButtonAllColor(defaultColor);

        yield return wait1Second;
        yield return wait1Second;
        yield return wait1Second;

        StartReturnChoice();

    }
    /// <summary>
    /// 정답 시도 중 틀렸을때 나오는 힌트
    /// </summary>
    /// <returns></returns>
    public void StartSetHintColor()
    {
        StartCoroutine(SetHintColor());
    }
    IEnumerator SetHintColor()
    {
        yield return waitForFixedUpdate;
        for (int i = currentButtonIndex + 1; i < 13; i++)
        {
            setcolor = GetColor(ScoreManager.Instance.anwsers[i - 1]);
            SendSerialMessage(i, $"{ScoreManager.Instance.anwsers[i - 1]},{setcolor.r},{setcolor.g},{setcolor.b}");
            yield return waitForFixedUpdate;
        }
    }

    /// <summary>
    /// Idle 상태 한줄씩 나오는 LED 코루틴 
    /// </summary>
    /// <returns></returns>

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
            string message1;
            for (int i = 1; i < 13; i++)
            {
                message1 = ReadSerialMessage(i);
                if (message1 != null)
                {
                    if (message1[0] != '_')

                        Debug.Log(message1);
                }


            }
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
