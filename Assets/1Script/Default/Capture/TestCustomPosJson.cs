using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;



public class TestCustomPosJson : MonoBehaviour
{

    public string tagNames = "";

    // 저장/로드할 JSON 파일명
    public string fileName = "HierarchyData.json";

    // **계층을 저장할 루트 오브젝트** (Inspector에서 직접 할당)
    // 예: MainPages라는 오브젝트를 여기에 연결
    public Transform rootTransform;

    void Update()
    {
        // F1 누르면 저장, F3 누르면 로드
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveHierarchy();
        }
        // else if (Input.GetKeyDown(KeyCode.F3))
        // {
        //     LoadHierarchy();
        // }
    }

    /// <summary>
    /// 재귀적으로 자식을 탐색하며, 태그가 tagNames인 오브젝트만 NodeData로 생성합니다.
    /// </summary>
    NodeData BuildNodeData(Transform current)
    {
        // 현재 오브젝트가 태그 tagNames가 아니라면 저장하지 않고 null 반환
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
        // parent 자체에 대한 트랜스폼 정보 적용
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

            // 루트의 각 자식에 대해 BuildNodeData를 호출하여 태그가 CustomPos인 자식만 추가
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
            // 루트 자체가 CustomPos 태그가 있으면 전체 트리 저장
            rootNodeData = BuildNodeData(rootTransform);
        }

        // JSON 변환 (한 줄로 저장하려면 두 번째 인자를 false로)
        string json = JsonUtility.ToJson(rootNodeData, false);

        // StreamingAssets 폴더 아래에 저장 (예: Application.streamingAssetsPath/HierarchyData.json)
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
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

        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON 파일이 존재하지 않습니다: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        NodeData rootNodeData = JsonUtility.FromJson<NodeData>(json);

        if (rootNodeData.Name != rootTransform.name)
        {
            Debug.LogWarning($"JSON의 루트 이름({rootNodeData.Name})과 현재 rootTransform({rootTransform.name})이 다릅니다.");
        }

        ApplyNodeData(rootTransform, rootNodeData);

        Debug.Log($"계층 구조 JSON 로드 및 적용 완료: {filePath}");
    }
}
