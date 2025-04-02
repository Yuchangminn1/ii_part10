using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Newtonsoft.Json용 Vector3 Converter
public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(Vector3);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector3 vec = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vec.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vec.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vec.z);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        float x = 0f, y = 0f, z = 0f;
        JObject obj = JObject.Load(reader);
        if (obj["x"] != null)
            x = obj["x"].Value<float>();
        if (obj["y"] != null)
            y = obj["y"].Value<float>();
        if (obj["z"] != null)
            z = obj["z"].Value<float>();
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class GameObjectData
{
    public string Key;            // 게임오브젝트 이름
    public Vector3 LocalPosition; // 로컬 위치
    public Vector3 LocalRotation; // 로컬 회전 (Euler 각도)
    public Vector3 LocalScale;    // 로컬 스케일
}

[System.Serializable]
public class NodeData
{
    public string Name;
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;   // Euler 각도
    public Vector3 LocalScale;
    public List<NodeData> Children;
}

public class CustomPosJson : MonoBehaviour
{
    // 태그 이름 (예: "CustomPos")
    public string tagNames = "CustomPos";

    // 저장/로드할 JSON 파일명
    string fileName = "CustomPos.json";

    // 계층을 저장할 루트 오브젝트 (Inspector에서 직접 할당)
    public Transform rootTransform;

    // Newtonsoft.Json 직렬화 설정 (Vector3Converter 포함)
    JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
    {
        Formatting = Formatting.None,
        Converters = new List<JsonConverter>() { new Vector3Converter() }
    };

    void Update()
    {
        //F3 누르면 저장, 필요시 다른 키로 로드
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SaveHierarchy();
        }
        // 필요 시 LoadHierarchy() 호출
        // else if (Input.GetKeyDown(KeyCode.F4))
        // {
        //     LoadHierarchy();
        // }
    }

    /// <summary>
    /// 재귀적으로 자식을 탐색하며, 태그가 tagNames인 오브젝트만 NodeData로 생성합니다.
    /// </summary>
    NodeData BuildNodeData(Transform current)
    {
        // 현재 오브젝트가 지정한 태그(tagNames)가 아니라면 저장하지 않고 null 반환
        if (!current.CompareTag(tagNames))
        {
            return null;
        }

        NodeData node = new NodeData();
        node.Name = current.name;
        node.LocalPosition = current.localPosition;
        node.LocalRotation = current.localEulerAngles;
        node.LocalScale = current.localScale;
        node.Children = new List<NodeData>();

        // 자식에 대해 재귀 호출
        for (int i = 0; i < current.childCount; i++)
        {
            NodeData childData = BuildNodeData(current.GetChild(i));
            if (childData != null)
            {
                node.Children.Add(childData);
            }
        }

        return node;
    }

    /// <summary>
    /// 계층 트리(NodeData)를 바탕으로 씬 내 오브젝트들의 트랜스폼 정보를 적용합니다.
    /// </summary>
    void ApplyNodeData(Transform parent, NodeData nodeData)
    {
        // parent 자체의 트랜스폼 정보 적용
        parent.localPosition = nodeData.LocalPosition;
        parent.localEulerAngles = nodeData.LocalRotation;
        parent.localScale = nodeData.LocalScale;

        // 자식 노드 순회하며 재귀 적용
        foreach (NodeData childData in nodeData.Children)
        {
            Transform childTransform = parent.Find(childData.Name);
            if (childTransform != null)
            {
                ApplyNodeData(childTransform, childData);
            }
            else
            {
                Debug.LogWarning($"[ApplyNodeData] '{parent.name}'의 자식 중 '{childData.Name}' 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// rootTransform부터 시작하여, 태그 tagNames인 오브젝트들만 JSON으로 저장합니다.
    /// </summary>
    public void SaveHierarchy()
    {
        if (rootTransform == null)
        {
            Debug.LogError("[SaveHierarchy] 루트 트랜스폼이 설정되지 않았습니다.");
            return;
        }

        NodeData rootNodeData;
        // 루트 자체가 태그 tagNames가 아니라면, 루트는 저장하지 않고 자식들만 저장
        if (!rootTransform.CompareTag(tagNames))
        {
            rootNodeData = new NodeData();
            rootNodeData.Name = rootTransform.name;
            rootNodeData.LocalPosition = rootTransform.localPosition;
            rootNodeData.LocalRotation = rootTransform.localEulerAngles;
            rootNodeData.LocalScale = rootTransform.localScale;
            rootNodeData.Children = new List<NodeData>();

            // 루트의 각 자식에 대해 BuildNodeData 호출 (태그가 일치하는 자식만 추가)
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                NodeData childData = BuildNodeData(rootTransform.GetChild(i));
                if (childData != null)
                {
                    rootNodeData.Children.Add(childData);
                }
            }
        }
        else
        {
            // 루트 자체가 tagNames 태그를 가지고 있으면 전체 트리 저장
            rootNodeData = BuildNodeData(rootTransform);
        }

        // Newtonsoft.Json을 사용하여 JSON 문자열 생성 (한 줄로 저장하려면 Formatting.None)
        // 여기서는 들여쓰기 포함하여 저장(Formatting.Indented)
        string json = JsonConvert.SerializeObject(rootNodeData, jsonSettings);

        // 파일 경로 생성 (StreamingAssets/Json 폴더 아래)
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", fileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"계층 구조 JSON 저장 완료: {filePath}");
    }

    /// <summary>
    /// JSON 파일을 읽어, rootTransform 아래로 트랜스폼 정보를 적용합니다.
    /// </summary>
    public void LoadHierarchy()
    {
        if (rootTransform == null)
        {
            Debug.LogError("[LoadHierarchy] 루트 트랜스폼이 설정되지 않았습니다.");
            return;
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON 파일이 존재하지 않습니다: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        NodeData rootNodeData = JsonConvert.DeserializeObject<NodeData>(json, jsonSettings);

        if (rootNodeData.Name != rootTransform.name)
        {
            Debug.LogWarning($"JSON의 루트 이름({rootNodeData.Name})과 현재 rootTransform({rootTransform.name})이 다릅니다.");
        }

        ApplyNodeData(rootTransform, rootNodeData);

        Debug.Log($"계층 구조 JSON 로드 및 적용 완료: {filePath}");
    }
}
