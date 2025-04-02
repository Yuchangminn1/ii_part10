using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// 문자열 내 숫자와 문자를 구분하여 자연스러운 정렬을 제공하는 NaturalComparer 클래스
public class NaturalComparer : IComparer<string>
{
    public int Compare(string a, string b)
    {
        if (a == b) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        // 문자열을 숫자와 문자로 분리
        string[] aParts = Regex.Split(a, "([0-9]+)");
        string[] bParts = Regex.Split(b, "([0-9]+)");

        int minParts = System.Math.Min(aParts.Length, bParts.Length);
        for (int i = 0; i < minParts; i++)
        {
            int result = 0;
            if (int.TryParse(aParts[i], out int aNum) && int.TryParse(bParts[i], out int bNum))
            {
                result = aNum.CompareTo(bNum);
            }
            else
            {
                result = string.Compare(aParts[i], bParts[i], System.StringComparison.Ordinal);
            }

            if (result != 0)
                return result;
        }
        return aParts.Length.CompareTo(bParts.Length);
    }
}

[System.Serializable]
public class HierarchicalImageData
{
    public string Key;               // 오브젝트 이름
    public Vector3 LocalPosition;    // 로컬 위치
    public Vector3 LocalRotation;    // 로컬 회전 (Euler 각도)
    public Vector3 LocalScale;       // 로컬 스케일

    // 현재 오브젝트에 Image 컴포넌트가 있으면 true, 없으면 false
    public bool HasImage;
    public int Width;                // RectTransform의 rect.width 값 (Image가 있을 경우)
    public int Height;               // RectTransform의 rect.height 값 (Image가 있을 경우)

    public List<HierarchicalImageData> Children;
}

public class ImageJsonSaver : MonoBehaviour
{
    // 저장할 JSON 파일명 (StreamingAssets 폴더에 저장됨)
    public string fileName = "ImagesHierarchy.json";

    // 계층 저장의 시작점이 되는 루트 오브젝트 (Inspector에서 할당)
    public Transform rootTransform;

    void Update()
    {
        // F1 키 입력 시 계층 구조 저장
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveImagesHierarchy();
        }
        // F2 키 입력 시 저장된 JSON 적용
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadAndApplyJson();
        }
    }

    /// <summary>
    /// 루트 오브젝트부터 재귀적으로 Image 컴포넌트가 있거나 자식 중 Image가 있는 노드가 있다면 부모도 포함하여 계층 데이터를 생성한 후 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveImagesHierarchy()
    {
        if (rootTransform == null)
        {
            Debug.LogError("루트 트랜스폼이 설정되지 않았습니다.");
            return;
        }

        HierarchicalImageData rootData = BuildImageHierarchyRecursive(rootTransform);
        if (rootData == null)
        {
            Debug.LogWarning("저장할 Image 컴포넌트가 있는 오브젝트가 없습니다.");
            return;
        }

        // 들여쓰기 없이 한 줄로 저장 (두 번째 매개변수를 false로 전달)
        string json = JsonUtility.ToJson(rootData, false);
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        File.WriteAllText(filePath, json);

        Debug.Log("이미지 계층 구조 JSON 저장 완료: " + filePath);
    }

    /// <summary>
    /// 재귀적으로 현재 노드와 자식 노드 중 Image 컴포넌트를 가진 것이 있으면 현재 노드를 생성하여 반환합니다.
    /// </summary>
    HierarchicalImageData BuildImageHierarchyRecursive(Transform current)
    {
        // 자식들을 먼저 재귀적으로 처리하여 유효한 자식 노드를 수집합니다.
        List<HierarchicalImageData> validChildNodes = new List<HierarchicalImageData>();
        foreach (Transform child in current)
        {
            HierarchicalImageData childNode = BuildImageHierarchyRecursive(child);
            if (childNode != null)
            {
                validChildNodes.Add(childNode);
            }
        }

        // 현재 노드가 Image 컴포넌트를 가지고 있는지 확인
        bool currentHasImage = current.GetComponent<Image>() != null;

        // 현재 노드와 자식들 중 하나라도 Image가 있다면 현재 노드를 생성
        if (!currentHasImage && validChildNodes.Count == 0)
        {
            // 현재 노드와 자식 중에 Image가 없다면 저장하지 않음
            return null;
        }

        // 현재 노드를 생성합니다.
        HierarchicalImageData node = new HierarchicalImageData();
        node.Key = current.gameObject.name;
        node.LocalPosition = current.localPosition;
        node.LocalRotation = current.localEulerAngles;
        node.LocalScale = current.localScale;
        node.HasImage = currentHasImage;
        if (currentHasImage)
        {
            RectTransform rect = current.GetComponent<RectTransform>();
            if (rect != null)
            {
                node.Width = Mathf.RoundToInt(rect.rect.width);
                node.Height = Mathf.RoundToInt(rect.rect.height);
            }
            else
            {
                node.Width = 0;
                node.Height = 0;
            }
        }
        else
        {
            node.Width = 0;
            node.Height = 0;
        }

        node.Children = validChildNodes;
        if (node.Children.Count > 0)
        {
            NaturalComparer comparer = new NaturalComparer();
            node.Children.Sort((a, b) => comparer.Compare(a.Key, b.Key));
        }

        return node;
    }

    /// <summary>
    /// JSON 파일을 읽어, 저장된 계층 데이터를 기반으로 씬 내 오브젝트에 트랜스폼 및 이미지 정보를 적용합니다.
    /// </summary>
    public void LoadAndApplyJson()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError("JSON 파일이 존재하지 않습니다: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        HierarchicalImageData rootData = JsonUtility.FromJson<HierarchicalImageData>(json);

        ApplyImageData(rootTransform, rootData);
        Debug.Log("JSON 파일의 데이터를 적용했습니다: " + filePath);
    }

    /// <summary>
    /// 저장된 HierarchicalImageData를 기반으로 씬 내 오브젝트에 트랜스폼 및 이미지 정보를 적용합니다.
    /// </summary>
    void ApplyImageData(Transform current, HierarchicalImageData data)
    {
        if (current.gameObject.name != data.Key)
        {
            Debug.LogWarning("오브젝트 이름이 일치하지 않습니다: " + current.gameObject.name + " vs " + data.Key);
        }

        current.localPosition = data.LocalPosition;
        current.localEulerAngles = data.LocalRotation;
        current.localScale = data.LocalScale;

        if (data.HasImage)
        {
            RectTransform rect = current.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(data.Width, data.Height);
            }
        }

        foreach (HierarchicalImageData childData in data.Children)
        {
            Transform childTransform = current.Find(childData.Key);
            if (childTransform != null)
            {
                ApplyImageData(childTransform, childData);
            }
            else
            {
                Debug.LogWarning("자식 오브젝트를 찾을 수 없습니다: " + childData.Key);
            }
        }
    }
}
