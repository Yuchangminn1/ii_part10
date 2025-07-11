using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
using System.Collections;
using UnityEngine.Networking;
using NUnit.Framework.Constraints;


//Json 데이터

#region JsonData


[System.Serializable]
public class PageTextData
{
    public string Key;
    public Vector3 LocalPosition;

    public float LocalScale;
    public string Text;

    public string Font;
    public int FontSize;
    public float LineSpacing;
    public _Alignment Alignment;
    public _Color Color;
}
[System.Serializable]
public class PageImageAndVideoData
{
    public string Key;
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale;
    public float Width;
    public float Height;


}

[System.Serializable]
public class ColorInt
{
    public int r;
    public int g;
    public int b;
    public int a;
}
[System.Serializable]
public class GeneralSettingData
{
    public string[] rfid_ports;
    public string[] ledPorts;
    public string api;
    public string baseURL;
    public string startURL;
    public string getUserDataURL;
    public string updateDataURL;
    public string clearURL;
    public int maxRetries;
    public float timeToWait;
    public string contentID;
    public int deviceID;
    public bool debugMode;
    public ColorInt[] arduinoState;

    int[] colors;

    public float QTimmer;
}



[System.Serializable]
public class _Alignment
{
    public string Vertical;
    public string Horizontal;
}

[System.Serializable]
public class _Color
{
    public int R;
    public int G;
    public int B;
    public int A;
}
[System.Serializable]
public class TextData
{
    public List<PageTextData> CustomTop;
    public List<PageTextData> Page0;
    public List<PageTextData> Page1;
    public List<PageTextData> Page2;
    public List<PageTextData> Page3;
    public List<PageTextData> Page4;
    public List<PageTextData> Page5;
    public List<PageTextData> Page6;
    public List<PageTextData> Page7;
    public List<PageTextData> Page8;
    public List<PageTextData> Page9;
    public List<PageTextData> Page10;
    public List<PageTextData> Page11;
    public List<PageTextData> Page12;
    public List<PageTextData> Page13;
    public List<PageTextData> CustomBottom;
}


[System.Serializable]
public class ImageAndVideoData
{
    public List<PageImageAndVideoData> CustomTop;

    public List<PageImageAndVideoData> Page0;
    public List<PageImageAndVideoData> Page1;
    public List<PageImageAndVideoData> Page2;
    public List<PageImageAndVideoData> Page3;
    public List<PageImageAndVideoData> Page4;
    public List<PageImageAndVideoData> Page5;
    public List<PageImageAndVideoData> Page6;
    public List<PageImageAndVideoData> Page7;
    public List<PageImageAndVideoData> Page8;
    public List<PageImageAndVideoData> Page9;
    public List<PageImageAndVideoData> Page10;
    public List<PageImageAndVideoData> Page11;
    public List<PageImageAndVideoData> Page12;
    public List<PageImageAndVideoData> Page13;
    public List<PageImageAndVideoData> CustomBottom;

}


#endregion

public class SetContentsManager : MonoBehaviour
{
    /*변경점
                1. Json 폴더 위치 /Json 추가
                2.      url = resourceAPI + pageNum + "_" + fileName+"."+ fileExtension;
                ->      url = resourceAPI + itemNum + "_" + fileName + "." + fileExtension;
             추후 변경 가능성 높음 */

    #region SetJson

    [SerializeField] private PageController pageController;
    [SerializeField] private GameObject mainPages;
    [SerializeField] private GameObject videoPlayers;

    [SerializeField]
    [Tooltip("세팅할 텍스트로 인식하는 태그")]
    private string[] textTagNames;

    [SerializeField]
    [Tooltip("세팅할 이미지로 인식하는 태그")]
    private string[] imageTagNames;

    [SerializeField]
    [Tooltip("세팅할 영상으로 인식하는 태그")]
    private string[] videoTagNames;

    [SerializeField]
    [Tooltip("Server API")]
    private string api = "http://211.110.44.104:8500/";

    [SerializeField]
    [Tooltip("서버 내 리소스 폴더명")]
    private string resourceFolderName = "resource";

    //Json폴더 만들어서 위치에 /Json추가 
    private string textSettingPath = "/StreamingAssets/Json/TextSetting.json";
    private string imageSettingPath = "/StreamingAssets/Json/ImageSetting.json";
    private string videoSettingPath = "/StreamingAssets/Json/VideoSetting.json";
    private string generalSettingPath = "/StreamingAssets/Json/GeneralSetting.json";
    private string resourcesFolderLocalPath = "/Resources/";
    private string resourceAPI = "";

    private const int ItemNum = 28;
    private const float UiDiv = 1f;

    private bool settingMode = true;
    private bool settingTextComplete = false;
    private List<bool> loadingImageComplete = new List<bool>();
    private bool loadingImageDataComplete = false;
    private List<bool> loadingVideoComplete = new List<bool>();
    private bool loadingVideoDataComplete = false;
    private bool generalSettingComplete = false;

    void Update()
    {
        if (settingMode)
        {
            bool loadingImageAllComplete = true;
            foreach (bool complete in loadingImageComplete)
            {
                if (!complete)
                {
                    loadingImageAllComplete = false;
                    break;
                }
            }

            bool loadingVideoAllComplete = true;
            foreach (bool complete in loadingVideoComplete)
            {
                if (!complete)
                {
                    loadingVideoAllComplete = false;
                    break;
                }
            }


            if (settingTextComplete && loadingImageAllComplete && loadingImageDataComplete && loadingVideoAllComplete &&
                loadingVideoDataComplete && generalSettingComplete)
            {
                settingMode = false;
                StartCoroutine(PageController.Instance.StartProgram());
            }
        }
    }

    public void StartSetting()
    {
        Reset();
        //DownloadAndSetContents();
        SetGeneralSetting();
    }

    private void Reset()
    {
        settingMode = true;
        settingTextComplete = false;
        loadingImageComplete = new List<bool>();
        loadingImageDataComplete = false;
        loadingVideoComplete = new List<bool>();
        loadingVideoDataComplete = false;
        generalSettingComplete = false;
    }

    public void DownloadAndSetContents()
    {
        //SetGeneralSetting(); //api 셋팅 있어서 가장 위
        // SetTexts();
        // SetImages();
        // SetVideos();
    }


    /// <summary>
    /// 'MainPages' 오브젝트 하위에 매 페이지마다 세팅할 텍스트들을 찾고 세팅한다. <br/><br/>
    /// 
    /// 1. 세팅 파일(JSON)을 불러오고, Class 파싱 <br/>
    /// 2. 세팅 파일에 명시된 텍스트 이름을 가진 텍스트 오브젝트가 그 페이지에 실제로 있는지 확인 <br/>
    /// 3. 명시된 이름을 가진 오브젝트가 존재한다면, Text 컴포넌트를 가지고 있는지 확인 <br/>
    /// 4. Text 컴포넌트를 가지고 있다면, 오브젝트의 태그가 에디터에서 설정한 textTagNames에 있는지 확인 <br/>
    /// 5. 위의 모든 조건을 충족하면 세팅
    /// 
    /// <br/><br/>참고<br/>
    ///  - 페이지 오브젝트 하위의 모든 자식 오브젝트, 자식의 자식 오브젝트 및 모든 자식 오브젝트 포함<br/>
    ///  - 활성화, 비활성화된 오브젝트 모두 포함<br/>
    /// </summary>
    private void SetTexts()
    {
        try
        {
            string path = Application.dataPath + textSettingPath;
            string str = File.ReadAllText(path);
            if (str != null)
            {
                TextData textData = JsonUtility.FromJson<TextData>(str.ToString());
                FieldInfo[] pages = typeof(TextData).GetFields();
                foreach (var page in pages)
                {
                    List<PageTextData> pageTexts =
                        (List<PageTextData>)textData.GetType().GetField(page.Name).GetValue(textData);
                    Transform pageObject = mainPages.transform.Find(page.Name);

                    foreach (var pageText in pageTexts)
                    {
                        GameObject currentSettingObject = null;

                        foreach (Transform child in pageObject.transform.GetComponentsInChildren<Transform>(true))
                        {
                            if (child.name == pageText.Key)
                            {
                                currentSettingObject = child.gameObject;
                                break;
                            }
                        }

                        if (currentSettingObject != null)
                        {
                            Text currentSettingText = null;
                            if (currentSettingObject.GetComponent<Text>() == null)
                            {
                                if (currentSettingObject.transform.Find("Text (Legacy)") != null)
                                {
                                    currentSettingText = currentSettingObject.transform.Find("Text (Legacy)")
                                        .GetComponent<Text>();
                                }
                            }
                            else
                            {
                                currentSettingText = currentSettingObject.GetComponent<Text>();
                            }

                            if (currentSettingText != null)
                            {
                                if (Array.Exists(textTagNames, name => name == currentSettingText.gameObject.tag))
                                {
                                    currentSettingText.text = pageText.Text;
                                    currentSettingText.transform.localScale = new Vector3(pageText.LocalScale,
                                        pageText.LocalScale, pageText.LocalScale);
                                    currentSettingText.transform.localPosition = new Vector3(pageText.LocalPosition.x,
                                        pageText.LocalPosition.y, pageText.LocalPosition.z);
                                    if (currentSettingText.transform.localPosition != Vector3.zero)
                                        currentSettingText.transform.localPosition /= UiDiv;
                                    currentSettingText.font = Resources.Load("Font/" + pageText.Font) as Font;
                                    currentSettingText.fontSize = pageText.FontSize = (int)(pageText.FontSize / UiDiv);
                                    currentSettingText.lineSpacing = pageText.LineSpacing;


                                    currentSettingText.alignment = (pageText.Alignment.Vertical,
                                              pageText.Alignment.Horizontal) switch
                                    {
                                        ("Top", "Left") => TextAnchor.UpperLeft,
                                        ("Top", "Center") => TextAnchor.UpperCenter,
                                        ("Top", "Right") => TextAnchor.UpperRight,
                                        ("Center", "Left") => TextAnchor.MiddleLeft,
                                        ("Center", "Center") => TextAnchor.MiddleCenter,
                                        ("Center", "Right") => TextAnchor.MiddleRight,
                                        ("Bottom", "Left") => TextAnchor.LowerLeft,
                                        ("Bottom", "Center") => TextAnchor.LowerCenter,
                                        ("Bottom", "Right") => TextAnchor.LowerRight,
                                        _ => TextAnchor.MiddleCenter
                                    };
                                    currentSettingText.color = new Color(pageText.Color.R / 255f,
                                        pageText.Color.G / 255f, pageText.Color.B / 255f, pageText.Color.A / 255f);
                                }
                            }
                        }
                    }
                }
            }

            settingTextComplete = true;
            //Debug.Log("Load Text Success");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }


    /// <summary>
    /// 'MainPages' 오브젝트 하위에 매 페이지마다 세팅할 이미지들을 서버로부터 받고, 이미지 관련 세팅파일을 파싱, 세팅한다.<br/><br/>
    /// 
    /// 1. 매 페이지마다 imageTagNames에 존재하는 태그를 가진 오브젝트가 있는지 확인<br/>
    /// 2. 해당 페이지의 해당 오브젝트명으로 서버에 이미지 API 호출<br/>
    /// 3. 내려받은 이미지 PC 로컬 경로에 저장<br/>
    ///         - 로컬 경로 : C:\Users\(사용자명)\AppData\LocalLow\(CompanyName)\(프로젝트명)\Resources <br/>
    /// 4. Image 데이터 세팅 JSON 파일을 파싱하여 매 페이지마다 이미지 데이터 설정 (서버로부터 JSON 파일 내려받는 로직 추가 예정)
    /// 
    /// <br/><br/>참고<br/>
    ///  - 페이지 오브젝트 하위의 모든 자식 오브젝트, 자식의 자식 오브젝트 및 모든 자식 오브젝트 포함<br/>
    ///  - 활성화, 비활성화된 오브젝트 모두 포함<br/>
    /// </summary>
    private void SetImages()
    {
        try
        {
            for (int i = 0; i < pageController.pages.Length; i++)
            {
                foreach (Transform child in pageController.pages[i].transform.GetComponentsInChildren<Transform>(true))
                {
                    if (Array.Exists(imageTagNames, name => name == child.gameObject.tag))
                    {
                        // int pageNum = int.Parse(pageController.pages[i].name[pageController.pages[i].name.Length - 1]
                        //     .ToString());
                        int pageNum = pageController.pages[i].GetComponent<PageSequenceController>().pageNumber;


                        GameObject imageObj = child.gameObject;

                        string fileName = imageObj.name;
                        string fileExtension = imageObj.tag;

                        string url = resourceAPI + ServerData.Instance.FindData(fileName);


                        StartCoroutine(LoadImage(pageNum, fileName + "." + fileExtension, url));
                    }
                }
            }

            SetImageDatas();
        }
        catch (Exception e)
        {
            //Debug.Log("Cannot Find : " + Url)
            Debug.LogError(e);
        }
    }



    private IEnumerator LoadImage(int PageNum, string fileName, string url)
    {



        //Debug.Log("LoadImage");
        bool loadingComplete = false;
        int completeIdx = loadingImageComplete.Count;
        loadingImageComplete.Add(loadingComplete);

        if (!Directory.Exists(Application.persistentDataPath + resourcesFolderLocalPath + "Page" + PageNum + "/"))
        {
            Directory.CreateDirectory(
                Application.persistentDataPath + resourcesFolderLocalPath + "Page" + PageNum + "/");
        }

        var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        var dh = new DownloadHandlerFile(
            Application.persistentDataPath + resourcesFolderLocalPath + "Page" + PageNum + "/" + fileName, false);
        dh.removeFileOnAbort = true;
        www.downloadHandler = dh;

        yield return www.SendWebRequest();


        // if (www.result is UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.ConnectionError )
        // {
        //     Debug.LogError(www.error);
        // }
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);

        }
        else
        {
            dh.Dispose();
            www.Dispose();
            loadingImageComplete[completeIdx] = true;
            // Debug.Log("Load Image Success");
        }

    }





    private void SetImageDatas()
    {
        string path = Application.dataPath + imageSettingPath;
        string str = File.ReadAllText(path);
        if (str != null)
        {
            ImageAndVideoData imageData = JsonUtility.FromJson<ImageAndVideoData>(str.ToString());
            FieldInfo[] pages = typeof(ImageAndVideoData).GetFields();
            foreach (var page in pages)
            {
                List<PageImageAndVideoData> pageImages =
                    (List<PageImageAndVideoData>)imageData.GetType().GetField(page.Name).GetValue(imageData);
                Transform pageObject = mainPages.transform.Find(page.Name);

                foreach (var pageImage in pageImages)
                {
                    GameObject currentSettingObject = null;

                    foreach (Transform child in pageObject.transform.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == pageImage.Key)
                        {
                            currentSettingObject = child.gameObject;
                            break;
                        }
                    }

                    if (currentSettingObject != null)
                    {
                        Image currentSettingImage = null;
                        if (currentSettingObject.GetComponent<Image>() != null)
                        {
                            currentSettingImage = currentSettingObject.GetComponent<Image>();
                        }

                        if (currentSettingImage != null)
                        {
                            if (Array.Exists(imageTagNames, name => name == currentSettingImage.gameObject.tag))
                            {
                                currentSettingImage.transform.localPosition = new Vector3(pageImage.LocalPosition.x,
                                    pageImage.LocalPosition.y, pageImage.LocalPosition.z);
                                if (currentSettingImage.transform.localPosition != Vector3.zero)
                                    currentSettingImage.transform.localPosition /= UiDiv;
                                currentSettingImage.GetComponent<RectTransform>().sizeDelta =
                                    new Vector2(pageImage.Width, pageImage.Height) / UiDiv;
                                currentSettingImage.transform.localRotation = Quaternion.Euler(
                                    new Vector3(pageImage.LocalRotation.x, pageImage.LocalRotation.y,
                                        pageImage.LocalRotation.z));
                                currentSettingImage.transform.localScale = new Vector3(pageImage.LocalScale.x,
                                    pageImage.LocalScale.y, pageImage.LocalScale.z);
                            }
                        }
                    }
                }
            }
        }

        loadingImageDataComplete = true;
        //Debug.Log("Load Image Data Success");
    }

    /// <summary>
    /// PageController에서 페이지를 열 때마다 해당 페이지의 이미지들을 로컬 경로에서 불러온다.
    /// 
    /// <br/><br/>참고<br/>
    ///  - 페이지 오브젝트 하위의 모든 자식 오브젝트, 자식의 자식 오브젝트 및 모든 자식 오브젝트 포함<br/>
    ///  - 활성화, 비활성화된 오브젝트 모두 포함<br/>
    /// </summary>
    public void SetImageByPage(GameObject _currentPageObj)
    {
        try
        {
            DestroyImagesOnMemory();
            string pageFolderName = _currentPageObj.name;

            foreach (Transform child in _currentPageObj.transform.GetComponentsInChildren<Transform>(true))
            {
                if (Array.Exists(imageTagNames, name => name == child.gameObject.tag))
                {
                    GameObject imageObj = child.gameObject;
                    string fileName = imageObj.name;
                    string fileExtension = imageObj.tag;
                    string path = Application.persistentDataPath + resourcesFolderLocalPath + "/" + pageFolderName +
                                  "/" + fileName + "." + fileExtension;

                    if (File.Exists(path))
                    {
                        Sprite settingSprite = LoadNewSprite(path);
                        currentSettingSprites.Add(settingSprite);
                        imageObj.GetComponent<Image>().sprite = settingSprite;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {
        Texture2D SpriteTexture = LoadTexture(FilePath);
        //추가
        if (SpriteTexture.width % 4 != 0 || SpriteTexture.height % 4 != 0)
        {
            SpriteTexture =
                ResizeTexture
                (
                    SpriteTexture,
                    Mathf.CeilToInt(SpriteTexture.width / 4.0f) * 4,
                    Mathf.CeilToInt(SpriteTexture.height / 4.0f) * 4
                );
        }

        CompressTexture(SpriteTexture);

        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),
            new Vector2(0, 0), PixelsPerUnit);
        return NewSprite;
    }

    private Texture2D LoadTexture(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData))
                return Tex2D;
        }

        return null;
    }

    /// <summary>
    /// 텍스쳐 최적화 진행
    /// </summary>
    /// <param name="texture">가져온 텍스쳐</param>
    private void CompressTexture(Texture2D texture)
    {
        // Compress의 경우 가져오는 texture의 크기가 4의 배수가 아니면 작동을 하지 않음
        texture.Compress(true);
        // makeNoLongerReadable의 경우 Sprite를 만들 때 추가적인 작업이 필요한 경우 false로 두고 Sprite 작업을 완료 후 true로 만든다
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
    }

    /// <summary>
    /// 텍스쳐의 사이즈를 변경한다. Full HD해상도 + 4의 배수로 바꾼다.
    /// </summary>
    /// <param name="texture">원본 텍스쳐</param>
    /// <param name="width">목표 width</param>
    /// <param name="height">목표 height</param>
    private Texture2D ResizeTexture(Texture2D texture, int width, int height)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = renderTexture;

        Graphics.Blit(texture, renderTexture);

        Texture2D result = new Texture2D(width, height, texture.format, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        return result;
    }


    /// <summary>
    /// PageController에서 페이지를 열 때마다 Memory에 올라가 있는 이미지들을 모두 제거한다.
    /// </summary>
    private List<Sprite> currentSettingSprites = new List<Sprite>();

    private void DestroyImagesOnMemory()
    {
        if (currentSettingSprites != null)
        {
            foreach (Sprite sp in currentSettingSprites)
            {
                Destroy(sp.texture);
            }

            currentSettingSprites = new List<Sprite>();
        }
    }


    /// <summary>
    /// 'VideoPlayers' 오브젝트 하위에 매 페이지마다 세팅할 영상을 서버로부터 받고, 영상 관련 세팅파일을 파싱 및 세팅한다.<br/><br/>
    /// 
    /// 1. VideoPlayers 오브젝트 하위에 Video Player 컴포넌트를 가지고 있는 오브젝트 확인<br/>
    /// 2. 그 오브젝트의 페이지 위치와 오브젝트 이름을 가지고 서버로부터 해당 영상 API 호출<br/>
    /// 3. 내려받은 영상을 PC 로컬 경로에 저장<br/>
    ///         - 로컬 경로 : C:\Users\(사용자명)\AppData\LocalLow\(CompanyName)\(프로젝트명)\Resources <br/>
    /// 4. 영상 관련 JSON 세팅 파일을 파싱, 세팅 (서버로부터 JSON 파일 받아오는 로직 추가 예정)
    /// 
    /// 참고<br/>
    ///  - 활성화, 비활성화된 오브젝트 모두 포함<br/>
    /// </summary>
    private void SetVideos()
    {
        try
        {
            foreach (Transform obj in videoPlayers.transform.GetComponentsInChildren<Transform>(true))
            {
                VideoPlayer vp = obj.GetComponent<VideoPlayer>();
                if (vp != null)
                {
                    //int pageNum = int.Parse(obj.parent.name[obj.parent.name.Length - 1].ToString());
                    int pageNum = obj.parent.GetComponent<PageNumData>().pageNum;

                    string fileName = obj.name;
                    string fileExtension = obj.tag;
                    string url = resourceAPI + ServerData.Instance.FindData(fileName);
                    //Debug.Log($"url = {url}");
                    StartCoroutine(LoadVideo(pageNum, fileName + "." + fileExtension, url));
                }
            }

            SetVideoDatas();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private IEnumerator LoadVideo(int PageNum, string fileName, string url)
    {
        // 비디오 로딩 완료 상태를 저장하는 변수 (초기값 false)를 생성
        bool loadingComplete = false;
        // 현재 로딩 완료 상태를 저장할 리스트의 인덱스를 구함
        int completeIdx = loadingVideoComplete.Count;
        // 로딩 완료 리스트에 현재 상태(false)를 추가
        loadingVideoComplete.Add(loadingComplete);

        // GET 방식으로 요청할 UnityWebRequest를 생성 (url에 해당하는 리소스 다운로드)
        var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);

        // DownloadHandlerFile를 생성하여 파일을 저장할 경로를 지정
        // Application.persistentDataPath: 앱의 영속적 데이터 경로
        // resourcesFolderLocalPath + "Page" + PageNum + "/" + fileName: 지정된 폴더 내에 파일을 저장
        // 두 번째 파라미터 false는 다운로드 중 파일을 덮어쓰지 않도록 설정
        var dh = new DownloadHandlerFile(
            Application.persistentDataPath + resourcesFolderLocalPath + "Page" + PageNum + "/" + fileName, false);

        // 다운로드 도중 요청이 중단되면 파일을 삭제하도록 설정
        dh.removeFileOnAbort = true;
        // UnityWebRequest에 DownloadHandlerFile 할당
        www.downloadHandler = dh;

        // 웹 요청을 보내고 요청이 완료될 때까지 기다림
        yield return www.SendWebRequest();

        // 네트워크 오류 또는 HTTP 오류가 발생했는지 확인
        if (www.isNetworkError || www.isHttpError)
        {
            // 오류 발생 시 에러 메시지 출력
            Debug.LogError(www.error);
        }
        else
        {
            // 오류 없이 정상적으로 다운로드가 완료된 경우,
            // 다운로드 핸들러와 웹 요청 객체의 리소스 해제
            dh.Dispose();
            www.Dispose();
            // 로딩 완료 리스트의 해당 인덱스를 true로 변경하여 로딩 완료 상태 표시
            loadingVideoComplete[completeIdx] = true;
        }
    }


    private void SetVideoDatas()
    {
        string path = Application.dataPath + videoSettingPath;
        string str = File.ReadAllText(path);
        if (str != null)
        {
            ImageAndVideoData videoData = JsonUtility.FromJson<ImageAndVideoData>(str.ToString());
            FieldInfo[] pages = typeof(ImageAndVideoData).GetFields();
            foreach (var page in pages)
            {
                List<PageImageAndVideoData> pageVideos =
                    (List<PageImageAndVideoData>)videoData.GetType().GetField(page.Name).GetValue(videoData);
                Transform pageObject = mainPages.transform.Find(page.Name);

                foreach (var pageVideo in pageVideos)
                {
                    GameObject currentSettingObject = null;

                    foreach (Transform child in pageObject.transform.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == pageVideo.Key)
                        {
                            currentSettingObject = child.gameObject;
                            break;
                        }
                    }

                    if (currentSettingObject != null)
                    {
                        RawImage currentSettingVideo = null;
                        if (currentSettingObject.GetComponent<RawImage>() != null)
                        {
                            currentSettingVideo = currentSettingObject.GetComponent<RawImage>();
                        }

                        if (currentSettingVideo != null)
                        {
                            if (Array.Exists(videoTagNames, name => name == currentSettingVideo.gameObject.tag))
                            {
                                currentSettingVideo.transform.localPosition = new Vector3(pageVideo.LocalPosition.x,
                                    pageVideo.LocalPosition.y, pageVideo.LocalPosition.z);
                                if (currentSettingVideo.transform.localPosition != Vector3.zero)
                                    currentSettingVideo.transform.localPosition /= UiDiv;
                                currentSettingVideo.GetComponent<RectTransform>().sizeDelta =
                                    new Vector2(pageVideo.Width, pageVideo.Height) / UiDiv;
                                currentSettingVideo.transform.localRotation = Quaternion.Euler(
                                    new Vector3(pageVideo.LocalRotation.x, pageVideo.LocalRotation.y,
                                        pageVideo.LocalRotation.z));
                                currentSettingVideo.transform.localScale = new Vector3(pageVideo.LocalScale.x,
                                    pageVideo.LocalScale.y, pageVideo.LocalScale.z);
                            }
                        }
                    }
                }
            }
        }

        loadingVideoDataComplete = true;
        //Debug.Log("Load Video Data Success");
    }

    /// <summary>
    /// PageController에서 페이지를 열 때마다 해당 페이지의 비디오의 로컬 경로 URL을 세팅한다.
    /// 
    /// <br/><br/>참고<br/>
    ///  - 페이지 오브젝트 하위의 모든 자식 오브젝트, 자식의 자식 오브젝트 및 모든 자식 오브젝트 포함<br/>
    ///  - 활성화, 비활성화된 오브젝트 모두 포함<br/>
    /// </summary>
    public void SetVideoByPage(GameObject currentVideoPageObj)
    {
        Debug.Log("SetVideoByPage");

        try
        {
            // DestroyImagesOnMemory();
            string pageFolderName = currentVideoPageObj.name;

            foreach (Transform obj in currentVideoPageObj.transform.GetComponentsInChildren<Transform>(true))
            {
                VideoPlayer vp = obj.GetComponent<VideoPlayer>();
                if (vp != null)
                {
                    //string pageNum = obj.parent.name;
                    string pageNum = obj.parent.GetComponent<PageNumData>().pageNum.ToString();

                    string fileName = obj.name;
                    string fileExtension = obj.tag;
                    string url = Application.persistentDataPath + resourcesFolderLocalPath + "Page" + pageNum + "/" + fileName + "." + fileExtension;
                    if (File.Exists(url))
                    {
                        vp.url = url;
                    }
                    else
                    {
                        Debug.LogWarning("영상 파일이 없음");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    #endregion

    [Space]
    [Space]
    [Space]
    [Header("------------General Setting (프로그램마다 필요한 값들 세팅)------------")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private GameObject putYourClassToSet;
    [SerializeField] Image[] customImages;


    [SerializeField] SequenceTag sequenceTag;
    [Header("필요한 위치값 Json 로딩")]
    [SerializeField] CustomPosJson customPosJson;

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    public TimmerScript timmerScript;


    public void SetGeneralSetting()
    {
        StartCoroutine(SetGeneralSetting2());
    }

    public IEnumerator SetGeneralSetting2()
    {
        string jsonContent = "";
        string path = "";

#if UNITY_IOS && !UNITY_EDITOR
        // iOS 빌드일 경우: Application.dataPath 기준으로 Raw/Json 폴더의 파일 사용
        path = Path.Combine(Application.streamingAssetsPath, "Json", "GeneralSetting.JSON");
        Debug.Log(" IOS 용 GeneralSetting 파일 경로: " + path);

        jsonContent = System.IO.File.ReadAllText(path);
        yield return null;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // iOS가 아닐 경우: Application.streamingAssetsPath를 사용
        path = Path.Combine(Application.streamingAssetsPath, "Json", "GeneralSetting.JSON");
        Debug.Log("Mac 용 GeneralSetting 파일 경로: " + path);

        jsonContent = System.IO.File.ReadAllText(path);
        yield return null;
#else
        path = Path.Combine(Application.streamingAssetsPath, "Json", "GeneralSetting.JSON");
        Debug.Log("기타 플랫폼용 GeneralSetting 파일 경로: " + path);

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GeneralSetting.json 파일을 읽지 못했습니다: " + request.error);
            yield break;
        }
        jsonContent = request.downloadHandler.text;

#endif

        Debug.Log("GeneralSetting JSON 내용: " + jsonContent);
        GeneralSettingData generalSettingData = JsonUtility.FromJson<GeneralSettingData>(jsonContent);

        yield return waitForFixedUpdate;
        if (CustomSerialController.Instance != null)
        {
            CustomSerialController.Instance.Initialize(generalSettingData.ledPorts);

            Color[] colors = new Color[generalSettingData.arduinoState.Length];
            for (int i = 0; i < generalSettingData.arduinoState.Length; i++)
            {
                ColorInt ci = generalSettingData.arduinoState[i];
                // 정수형 값을 255로 나누어 0~1 범위의 float값으로 변환
                colors[i] = new Color(ci.r / 255f, ci.g / 255f, ci.b / 255f, ci.a / 255f);
            }
            // 변환된 Color 배열을 SetColor 메서드를 통해 상태 업데이트 및 출력
            CustomSerialController.Instance.SetColor(colors);
        }
        else
        {
            Debug.Log("ArduinoController Is Null");
        }
        if (sequenceTag != null)
        {
            sequenceTag.PortsInitialize(generalSettingData.rfid_ports);
        }
        api = generalSettingData.api;
        resourceAPI = api + resourceFolderName + "/";
        generalSettingComplete = true;

        SetTexts();
        SetImages();
        SetVideos();

        if (customPosJson == null) customPosJson = GetComponent<CustomPosJson>();
        if (customPosJson != null)
        {
            customPosJson.LoadHierarchy();
            Debug.Log("customPosJson.LoadHierarchy()");
        }
        if (customImages != null) MyImage(customImages);

        timmerScript.SetDefaultTime(generalSettingData.QTimmer);

        pageController.StartProgram();

        yield return new WaitForSeconds(5f);
        pageController.CurrentPage = 0;


    }

    public void MyImage(Image imageObj)
    {
        string fileName = imageObj.name;
        string url = resourceAPI + ServerData.Instance.FindData(fileName);
        StartCoroutine(LoadAndAssignImage(url, imageObj));
    }

    // Image 배열에 대해 각각의 Image에 이미지 다운로드 및 적용
    public void MyImage(Image[] imageObjs)
    {
        foreach (Image imageObj in imageObjs)
        {
            MyImage(imageObj);
        }
    }

    private IEnumerator LoadAndAssignImage(string url, Image targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("이미지 다운로드 실패: " + request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture,
                                              new Rect(0, 0, texture.width, texture.height),
                                              new Vector2(0.5f, 0.5f));
                targetImage.sprite = sprite;
            }
        }
    }

}