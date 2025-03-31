using System;
using System.Collections;
using UnityEngine;


public class PageController : MonoBehaviour
{
    //싱글턴

    #region Singleton

    private static PageController instance;

    public static PageController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PageController>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("PageController");
                    instance = singletonObject.AddComponent<PageController>();
                }
            }

            return instance;
        }
    }

    #endregion


    #region default

    [Space]
    [Space]
    [Space]
    [Header("-----------------Debug Mode----------------")]
    [SerializeField]
    private bool debugMode = false;

    [SerializeField] private int openingPage = 0;

    [Header("---------------------------------------------")]
    [SerializeField]
    [Tooltip("Pages 오브젝트 내부에 추가된 모든 Page 오브젝트")]
    public GameObject[] pages;

    [SerializeField]
    [Tooltip("VideoPlayers 오브젝트 내부에 추가된 모든 Page 오브젝트")]
    private GameObject[] videos;

    [SerializeField] private SetContentsManager setContentsManager;
    [SerializeField] private GameObject settingPage;

    [SerializeField] private int currentPage = 0;
    [SerializeField] private bool isTutorial = false;

    #endregion

    [Space]
    [Space]
    [Space]
    [Header("-----------프로그램마다 필요한 설정-----------")]
    [SerializeField]
    public Action<int> OnPageChange; //페이지 변경

    public Action<Action> OnPageRequest; //페이지 변경


    public Action OnRestRequest; //변수 리셋
    public Action OnUpdateRequest; //업데이트 (조건확인)

    //처음 초기화 기다리기
    public float setupDelayTime = 1f;
    private WaitForSeconds SetupDelay;

    public int CurrentPage
    {
        get { return currentPage; }
        set { CheckCondition(value); }
    }

    private void CheckCondition(int value)
    {

        if (OnPageRequest != null)
        {
            OnPageRequest.Invoke(() => OpenPage(value));
        }

        else
        {
            //Debug.Log($"OnPageRequest is null");
            OpenPage(value);
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SetupDelay = new WaitForSeconds(setupDelayTime);
    }

    void Start()
    {
        CloseAllPages();
        settingPage.SetActive(true);
        if (setContentsManager == null) StartCoroutine(StartProgram());
    }


    void Update()
    {
        //OpenPage - > CurrentPage프로퍼티 호출로 변경

        #region DebugMode

        if (debugMode)
        {
            if (Input.inputString.Length > 0)
            {
                char inputChar = Input.inputString[0];

                if (char.IsDigit(inputChar))
                {
                    CurrentPage = inputChar - '0';
                }
                else if (char.IsLetter(inputChar))
                {
                    inputChar = char.ToUpper(inputChar);
                    CurrentPage = 10 + (inputChar - 'A');
                }
            }
        }

        #endregion

        //페이지 넘기는 조건들 가진 스크립트 업데이트
        if (OnUpdateRequest == null) return;
        OnUpdateRequest?.Invoke();
    }

    public IEnumerator StartProgram()
    {
        //Debug.Log("StartProgram");
        yield return SetupDelay;
        currentPage = 8; // 처음 페이지 0->0 불가능해서 임시값
        //Debug.Log("Setting Completed");
        if (debugMode)
        {
            CurrentPage = openingPage;
        }
        else
        {
            CurrentPage = 0;
        }
    }

    private void Reset()
    {
        OnRestRequest?.Invoke();
    }

    public void OpenPage(int pageNum)
    {
        if (pages.Length > pageNum)
        {
            currentPage = pageNum;
            Reset();
            ChangeUI(pageNum);
            //Debug.Log($"OpenPage ");
            if (pages.Length - 1 == pageNum) //마지막 페이지 오픈시 클리어
            {
                UserDataManager.Instance.RequestUserContentEnd();
            }
            if (OnPageChange != null)
                OnPageChange.Invoke(pageNum);
        }
    }

    private void ChangeUI(int pageNum)
    {
        if (setContentsManager != null)
        {
            setContentsManager.SetImageByPage(pages[pageNum]);
            setContentsManager.SetVideoByPage(videos[pageNum]);
        }
        else
        {
            Debug.LogWarning("setContentManager is Null");
        }

        CloseAllPages();
        pages[pageNum].SetActive(true);
        videos[pageNum].SetActive(true);
    }

    private void CloseAllPages()
    {
        settingPage.SetActive(false);
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        foreach (GameObject video in videos)
        {
            video.SetActive(false);
        }
    }

    public bool GetDebugMode()
    {
        return debugMode;
    }
    #region PageButton
    public void DebugModeNextButton()
    {
        if (debugMode) NextButton();
    }


    public void DebugModePreviousButton()
    {
        if (debugMode) PreviousButton();
    }
    public void NextButton()
    {
        CurrentPage++;
    }

    public void PreviousButton()
    {
        CurrentPage--;
    }

    public void IdleButton()
    {
        CurrentPage = 0;
    }
    #endregion

}