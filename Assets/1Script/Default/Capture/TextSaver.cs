using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class HierarchicalTextData
{
    public string Key;              // 게임오브젝트 이름
    public Vector3 LocalPosition;   // 로컬 위치
    public float LocalScale;        // 균일 스케일 (x 값)

    // Text 컴포넌트 관련 데이터 (Text가 있을 경우에만 값이 채워짐)
    public string Text;
    public string Font;
    public int FontSize;
    public float LineSpacing;
    public _Alignment Alignment;
    public _Color Color;

    public List<HierarchicalTextData> Children;
}

public class TextSaver : MonoBehaviour
{
    // 저장될 JSON 파일명 (StreamingAssets 폴더 아래)
    public string fileName = "TextEntriesHierarchy.json";

    // 계층 저장의 시작점이 되는 루트 오브젝트. Inspector에서 할당하세요.
    public Transform rootTransform;

    void Update()
    {
        // F1 키 입력 시 계층구조 저장
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveTextsToJson();
        }
    }

    void SaveTextsToJson()
    {
        if (rootTransform == null)
        {
            Debug.LogError("루트 트랜스폼이 설정되지 않았습니다.");
            return;
        }

        // 루트부터 재귀적으로 Text 컴포넌트를 가진 오브젝트가 있으면 노드를 생성합니다.
        HierarchicalTextData rootData = BuildTextHierarchy(rootTransform);
        if (rootData == null)
        {
            Debug.LogWarning("저장할 Text 컴포넌트를 가진 오브젝트가 없습니다.");
            return;
        }

        // JSON 문자열로 변환 (들여쓰기 없이 한 줄로 저장)
        string json = JsonUtility.ToJson(rootData, false);
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        File.WriteAllText(path, json);

        Debug.Log("텍스트 계층 구조 저장 완료: " + path);
    }

    /// <summary>
    /// 현재 오브젝트 및 자식 중 Text 컴포넌트를 가진 것이 있으면 노드를 생성하여 반환합니다.
    /// 부모에 Text 컴포넌트가 없더라도 자식 중에 Text 컴포넌트가 있다면 부모 노드를 생성하여 계층구조를 유지합니다.
    /// </summary>
    HierarchicalTextData BuildTextHierarchy(Transform current)
    {
        // 자식 노드들을 재귀적으로 처리합니다.
        List<HierarchicalTextData> childNodes = new List<HierarchicalTextData>();
        foreach (Transform child in current)
        {
            HierarchicalTextData childNode = BuildTextHierarchy(child);
            if (childNode != null)
            {
                childNodes.Add(childNode);
            }
        }

        // 현재 오브젝트에 Text 컴포넌트가 있는지 확인합니다.
        Text textComp = current.GetComponent<Text>();
        bool hasText = (textComp != null);

        // 현재 오브젝트 또는 자식 중 하나라도 Text가 있다면 현재 노드를 생성합니다.
        if (hasText || childNodes.Count > 0)
        {
            HierarchicalTextData node = new HierarchicalTextData();
            node.Key = current.gameObject.name;
            node.LocalPosition = current.localPosition;
            node.LocalScale = current.localScale.x; // 균일 스케일 가정

            if (hasText)
            {
                node.Text = textComp.text;
                node.Font = (textComp.font != null) ? textComp.font.name : "";
                node.FontSize = textComp.fontSize;
                node.LineSpacing = textComp.lineSpacing;

                _Alignment align = new _Alignment();
                switch (textComp.alignment)
                {
                    case TextAnchor.UpperLeft:
                        align.Vertical = "Top";
                        align.Horizontal = "Left";
                        break;
                    case TextAnchor.UpperCenter:
                        align.Vertical = "Top";
                        align.Horizontal = "Center";
                        break;
                    case TextAnchor.UpperRight:
                        align.Vertical = "Top";
                        align.Horizontal = "Right";
                        break;
                    case TextAnchor.MiddleLeft:
                        align.Vertical = "Center";
                        align.Horizontal = "Left";
                        break;
                    case TextAnchor.MiddleCenter:
                        align.Vertical = "Center";
                        align.Horizontal = "Center";
                        break;
                    case TextAnchor.MiddleRight:
                        align.Vertical = "Center";
                        align.Horizontal = "Right";
                        break;
                    case TextAnchor.LowerLeft:
                        align.Vertical = "Bottom";
                        align.Horizontal = "Left";
                        break;
                    case TextAnchor.LowerCenter:
                        align.Vertical = "Bottom";
                        align.Horizontal = "Center";
                        break;
                    case TextAnchor.LowerRight:
                        align.Vertical = "Bottom";
                        align.Horizontal = "Right";
                        break;
                    default:
                        align.Vertical = "Center";
                        align.Horizontal = "Center";
                        break;
                }
                node.Alignment = align;

                _Color colorData = new _Color();
                colorData.R = Mathf.RoundToInt(textComp.color.r * 255);
                colorData.G = Mathf.RoundToInt(textComp.color.g * 255);
                colorData.B = Mathf.RoundToInt(textComp.color.b * 255);
                colorData.A = Mathf.RoundToInt(textComp.color.a * 255);
                node.Color = colorData;
            }
            else
            {
                // Text 컴포넌트가 없으면 기본값 할당 (빈 문자열 등)
                node.Text = "";
                node.Font = "";
                node.FontSize = 0;
                node.LineSpacing = 0;
                node.Alignment = null;
                node.Color = null;
            }

            node.Children = childNodes;
            return node;
        }
        // 현재 오브젝트와 자식들에 Text가 하나도 없다면 null 반환
        return null;
    }
}
