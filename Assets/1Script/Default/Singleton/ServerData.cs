using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class InitialData
{
    public List<string> COLUMNS;
    public List<List<object>> DATA;
}

public class ServerData : MonoBehaviour
{
    private static ServerData instance;
    public static ServerData Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ServerData>();
            return instance;
        }
    }

    int deviceNum = 1;
    public int DeviceNum { get { return deviceNum; } }
    string code = "10A";
    public string Code { get { return code; } }
    public event Action onCoroutineEnd;
    public Coroutine severCoroutine;

    private string _nonSerializedData;
    public InitialData initData;


    [SerializeField] SetContentsManager setContentsManager;



    void Awake()
    {
        onCoroutineEnd = ResetCoroutine;
        if (setContentsManager == null) setContentsManager = GetComponent<SetContentsManager>();

        StartCoroutine(StartProgram());
    }

    public string FindData(string objectName)
    {

        if (initData == null)
        {
            Debug.Log("initData IS Null ");
            return null;
        }
        if (initData.DATA == null)
        {
            Debug.Log("initData.DATA IS Null ");
            return null;
        }

        foreach (var t in initData.DATA) // 25번째 줄
        {
            if (t[2].Equals(objectName))
            {
                return t[3].ToString();
            }
        }
        Debug.LogError("Cannot Find Data : " + objectName);
        return null;
    }

    public IEnumerator StartProgram()
    {
        string urlStartApp = $"http://211.110.44.104:8500/api/logApp.cfm?status=run&code={code}&device={deviceNum}&";

        string urlLoadData = $"http://211.110.44.104:8500/dev/resourceJSON.cfm?code={code}";

        var www = UnityWebRequest.Get(urlStartApp);

        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        string jsonText = www.downloadHandler.text;

        Debug.Log(jsonText);

        www = UnityWebRequest.Get(urlLoadData);

        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        _nonSerializedData = www.downloadHandler.text;

        initData = JsonConvert.DeserializeObject<InitialData>(_nonSerializedData);

        setContentsManager.StartSetting();
    }

    void OnDisable()
    {
        string urlEndApp = $"http://211.110.44.104:8500/api/logApp.cfm?status=end&code={code}&device={deviceNum}&";

        var www = UnityWebRequest.Get(urlEndApp);

        www.downloadHandler = new DownloadHandlerBuffer();

        www.SendWebRequest();
    }

    public void ResetCoroutine()
    {
        if (severCoroutine != null)
        {
            StopCoroutine(severCoroutine);
            severCoroutine = null;
        }

    }

    public void RequestSeverData(string _url, Action<string> _callback)
    {
        if (severCoroutine == null)
        {
            severCoroutine = StartCoroutine(RequestDataCoroutine(_url, _callback));
        }
        else
        {
            Debug.Log("severCoroutine Is Working");
        }
    }



    IEnumerator RequestDataCoroutine(string _url, Action<string> _callback)
    {
        var www = UnityWebRequest.Get(_url);

        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        string jsonText = www.downloadHandler.text;

        //Debug.LogWarning(jsonText);

        _callback?.Invoke(jsonText);

        onCoroutineEnd?.Invoke();


    }

}
