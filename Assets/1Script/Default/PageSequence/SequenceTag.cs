using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class SequenceTag : SequenceScript
{
    public bool activeTagging = false; //인자

    [SerializeField] private SerialPort[] m_rfidReader; //시리얼 포트 여러개 받기

    //[SerializeField] private RFIDData[] rfidData;
    [SerializeField] private string[] RFIDPorts;
    [SerializeField] private bool[] m_isRFID;
    [SerializeField] private string[] RFIDMessage;
    [SerializeField] private bool[] m_isTag;

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    [SerializeField] private bool isTag = false;

    //public bool isInitialize = false;

    Coroutine waitTag = null;

    Coroutine testCoroutine = null;


    string iceEnd = "9C";
    string moleEnd = "10A";
    string houseEnd = "11B";
    string starEnd = "12A";

    #region RFIDFunction

    public void PortsInitialize(string[] _RFIDPorts)
    {
        Debug.Log($"_RFIDPorts = {_RFIDPorts.Length}");
        RFIDPorts = _RFIDPorts;
        m_isRFID = new bool[RFIDPorts.Length];
        m_rfidReader = new SerialPort[RFIDPorts.Length];
        RFIDMessage = new string[RFIDPorts.Length];
        m_isTag = new bool[RFIDPorts.Length];
        isInitialize = true;

    }

    void OnEnable()
    {
        if (isInitialize)
        {
            ActiveTagging(true);
        }
        isTag = false;
    }
    void OnDisable()
    {
        ActiveTagging(false);
        if (testCoroutine != null)
        {
            StopCoroutine(testCoroutine);
            testCoroutine = null;
        }

    }

    private void OpenPorts()
    {
        if (m_rfidReader == null) return;
        for (int i = 0; i < m_rfidReader.Length; i++)
        {
            m_rfidReader[i] = new SerialPort(RFIDPorts[i], 9600);
            m_isRFID[i] = false;
            try
            {
                m_rfidReader[i].Open();
                m_rfidReader[i].ReadTimeout = 10; // <- 이거 셋 안하면 무한대기상태 디폴트 -1 유니티 다운
                m_isRFID[i] = true;
                Debug.Log("[PortManager Debug]" + i + "번 RFID 포트 열기 성공");
            }
            catch (System.Exception e)
            {
                m_isRFID[i] = false;
                Debug.Log("[PortManager Debug]" + i + "번 RFID 포트 열기 실패 : " + e.Message);
            }
        }

    }

    private void ClosePorts()
    {
        if (m_rfidReader == null || m_rfidReader.Length < 1) return;

        for (int i = 0; i < m_rfidReader.Length; i++)
        {
            try
            {
                m_rfidReader[i].Close();
                Debug.Log("[PortManager Debug]" + i + "번 RFID 포트 닫기 성공");
            }
            catch (System.Exception e)
            {
                Debug.Log("[PortManager Debug]" + i + "번 RFID 포트 닫기 실패 : " + e.Message);
            }
        }
    }

    public void ActiveTagging(bool active)
    {
        // Debug.Log("QQ");
        if (active)
        {
            activeTagging = true;
            OpenPorts();
        }
        else
        {
            activeTagging = false;
            ClosePorts();
        }
    }


    protected override IEnumerator RunSequence()
    {
        while (!isTag)
        {
            if (activeTagging && m_rfidReader != null)
            {
                for (int i = 0; i < m_rfidReader.Length; i++)
                {
                    try
                    {
                        if (m_isRFID[i])
                        {
                            string data = m_rfidReader[i].ReadLine().Replace("\u0002", "").Trim();

                            Debug.Log($"Tag portP{i} : {data}");
                            UserDataManager.Instance.RequestInitializeUserData(data);
                            StartCoroutine(IsPlay());

                            //if (testCoroutine == null) testCoroutine = StartCoroutine(TestC());

                            //isTag = true;
                        }
                    }
                    catch (Exception e)
                    {
                        //Debug.Log("Error Tagging : " + e.Message);
                    }
                }
            }
            yield return null;
        }
    }
    protected override void AwakeSetup()
    {
        ;
    }

    IEnumerator IsPlay()
    {
        yield return new WaitForSeconds(0.1f);
        UserDataManager.Instance.RequestUserStartReset();
        //UserDataManager.Instance.RequestUserContentEnd();
        yield return new WaitForSeconds(0.1f);

        string Q = UserDataManager.Instance.FindValue("END_10A");

        if (Q != "null")
        {
            PopupScript.Instance.Popup(2);
            yield return new WaitForSeconds(5f);
            PopupScript.Instance.ResetIndex();
        }
        else
        {
            isTag = true;
        }
        //UserDataManager.Instance.RequestUserStartReset();
        //yield return new WaitForSeconds(0.1f);
    }

    #endregion

}