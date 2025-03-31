using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UserJsonData
{
    public List<string> COLUMNS { get; set; }
    public List<List<object>> DATA { get; set; }
}

public class UserDataManager : MonoBehaviour
{

    private static UserDataManager instance;

    public static UserDataManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<UserDataManager>();
            return instance;
        }
    }

    private Dictionary<string, string> userDataCache = null;

    private Action onUserUIDSet;

    const int contentNum = 4;

    public int ContentNum { get { return contentNum; } }

    public int[] stamp { get; private set; } = new int[contentNum];

    string[] endValues = { "9C", "10A", "11B", "12A" };

    //아이스크림 9C  10A 11B 12A  

    public void AddUserUIDSet(Action action)
    {
        onUserUIDSet += action;
    }

    public string FindValue(string _Key)
    {
        if (userDataCache == null)
        {
            Debug.Log(" userDataCache = null");
            return null;
        }
        return userDataCache[_Key];
    }

    public void RequestUserDataUpdate(int _question, string _value, string _code = null)
    {
        if (userDataCache == null) return;
        if (_code == null) _code = ServerData.Instance.Code;
        ServerData.Instance.RequestSeverData("http://211.110.44.104:8500/api/" + $"updateValue.cfm?idx_user={userDataCache["IDX_USER"]}&uid={userDataCache["UID"]}&code={_code}&question={_question}&value={_value}&device={1}", Answer);
    }


    public void RequestInitializeUserData(string userUID)
    {
        if (userDataCache == null) return;
        ServerData.Instance.RequestSeverData("http://211.110.44.104:8500/api/" + $"checkIDX.cfm?uid={userUID}&device={ServerData.Instance.DeviceNum}&Code={ServerData.Instance.Code}", ParseJsonData);
    }

    public void RequestUserStartReset()
    {
        if (userDataCache == null) return;
        ServerData.Instance.RequestSeverData($"http://211.110.44.104:8500/dev/resetTime.cfm?idx_user={userDataCache["IDX_USER"]}&code={ServerData.Instance.Code}", Answer);
    }

    public void RequestUserContentEnd()
    {
        if (userDataCache == null) return;
        //Debug.Log($"IDX_USER = {FindValue("IDX_USER")} UID = {FindValue("UID")}");
        ServerData.Instance.RequestSeverData($"http://211.110.44.104:8500/api/updateTime.cfm?status=end&idx_user={FindValue("IDX_USER")}&uid={FindValue("UID")}&code={ServerData.Instance.Code}&device={ServerData.Instance.DeviceNum}", Answer);
    }

    //http://211.110.44.104:8500/api/logApp.cfm?status=run&code=11&device=1&

    public void Answer(string _an)
    {
        Debug.Log("Server : " + _an);
    }

    public bool IsUser()
    {
        if (userDataCache != null && userDataCache.Count > 0)
        {
            return true;
        }
        return false;
    }

    public void ParseJsonData(string jsonText)
    {
        try
        {
            // 우선 클래스로 파싱
            UserJsonData parsedData = JsonConvert.DeserializeObject<UserJsonData>(jsonText);

            if (parsedData == null || parsedData.COLUMNS == null || parsedData.DATA == null || parsedData.DATA.Count == 0)
            {
                Debug.LogError("JSON 구조가 잘못되었습니다.");
                userDataCache = null;
                return; //false;
            }

            List<string> columns = parsedData.COLUMNS;
            List<object> dataRow = parsedData.DATA[0]; // 첫번째 데이터 행을 사용한다고 가정

            if (columns.Count != dataRow.Count)
            {
                Debug.LogError("COLUMNS와 DATA의 개수가 맞지 않습니다.");
                userDataCache = null;
                return;//false;
            }

            // Dictionary 생성
            userDataCache = new Dictionary<string, string>();

            for (int i = 0; i < columns.Count; i++)
            {
                string key = columns[i];
                string value = dataRow[i]?.ToString() ?? "null";
                userDataCache[key] = value;
            }
            if (userDataCache != null && userDataCache.Count > 0)
            {
                if (onUserUIDSet != null) onUserUIDSet.Invoke();
            }

            StampInitialize();

        }
        catch (JsonException ex)
        {
            Debug.LogError("JSON 파싱 중 에러 발생: " + ex.Message);
            userDataCache = null;
            return;// false;
        }
    }

    public int GetStamp(int _contentIDX)
    {
        if (stamp.Length > _contentIDX)
        {
            return stamp[_contentIDX];
        }

        else
        {
            Debug.Log("GetStamp Error ");
            return -1;
        }
    }

    private void StampInitialize()
    {
        // stamp 배열을 0으로 초기화 (이미 0으로 초기화되어 있다면 이 부분은 생략 가능)
        for (int i = 0; i < contentNum; i++)
        {
            stamp[i] = 0;
        }
        // 각 엔드 값에 대해 데이터 캐시에서 값 읽어오기
        for (int i = 0; i < endValues.Length; i++)
        {
            string key = $"VALUE_{endValues[i]}_1";
            if (userDataCache.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value) && value != "null")
            {
                // int.TryParse를 통해 안전하게 변환
                if (int.TryParse(value, out int parsedValue))
                {
                    stamp[i] = parsedValue;
                }
                else
                {
                    Debug.Log($"stamp{i} Error");
                }
            }
        }
    }
}