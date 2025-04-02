using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public class JsonSaverExample : MonoBehaviour
{
    // JSON 파일의 전체 경로를 입력하세요.
    // 예를 들어, StreamingAssets 폴더 내 파일의 경우:
    // Path.Combine(Application.streamingAssetsPath, "MyData.json");
    string filePath;
    string fileName = "TextSetting.json";
    //ImageSetting
    //TextSetting

    // F1 키를 누르면 해당 파일을 compact JSON 형식으로 저장합니다.

    void Awake()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "Json", fileName);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            CompactAndSaveJson(filePath);
        }
    }

    /// <summary>
    /// 주어진 파일 경로의 JSON 파일을 읽어, 줄바꿈과 들여쓰기를 제거한 compact JSON으로 다시 저장합니다.
    /// (내용의 데이터 구조는 그대로 유지됩니다.)
    /// </summary>
    /// <param name="filePath">JSON 파일의 전체 경로</param>
    public void CompactAndSaveJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("파일이 존재하지 않습니다: " + filePath);
            return;
        }

        // 파일에서 JSON 문자열 읽기
        string json = File.ReadAllText(filePath);

        // 정규식을 사용하여 줄바꿈(\r\n 또는 \n)과 뒤따르는 공백 제거
        string compactJson = Regex.Replace(json, @"\r?\n\s*", "");

        // compact JSON으로 파일 덮어쓰기
        File.WriteAllText(filePath, compactJson);

        Debug.Log("Compact JSON 저장 완료: " + filePath);
    }
}
