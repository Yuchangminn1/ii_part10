using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public class PageSequenceManager : MonoBehaviour
{
    private static PageSequenceManager instance;

    public static PageSequenceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PageSequenceManager>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("PageSequenceManager");
                    instance = singletonObject.AddComponent<PageSequenceManager>();
                }
            }

            return instance;
        }
    }

    // 페이지 번호와 컨트롤러를 매핑하는 딕셔너리
    private Dictionary<int, PageSequenceController> pageSequenceControllers =
        new Dictionary<int, PageSequenceController>();

    private PageSequenceController pageSequenceController;

    private void Start()
    {
        PageController.Instance.OnPageChange += UpdateCurrentController;
    }

    /// <summary>
    /// 페이지 컨트롤러를 등록 (각 페이지 컨트롤러가 Awake()에서 호출)
    /// </summary>
    public void RegisterController(PageSequenceController controller)
    {
        if (controller != null)
        {
            if (pageSequenceControllers.ContainsKey(controller.pageNumber))
            {
                Debug.LogWarning($"PageSequenceManager: 페이지 {controller.pageNumber}에 이미 컨트롤러가 등록되어 있습니다. 덮어씁니다.");
            }

            pageSequenceControllers[controller.pageNumber] = controller;
        }
    }

    public void UpdateCurrentController(int pageIndex)
    {
        if (pageSequenceControllers.TryGetValue(pageIndex, out var _controller))
        {
            if (PageController.Instance.OnPageRequest != null)
                PageController.Instance.OnPageRequest -= pageSequenceController.onEndPage;
            pageSequenceController = _controller;
            pageSequenceController.ChangePage();
            PageController.Instance.OnPageRequest += pageSequenceController.onEndPage;
        }
    }

    public PageSequenceController CurrentPageController()
    {
        if (pageSequenceController != null)
            return pageSequenceController;
        return null;
    }

    public void NextPage(int _num = -1)
    {
        if (_num == -1)
            PageController.Instance.CurrentPage++;
        else
            PageController.Instance.CurrentPage = _num;
    }

    public void NextSequence()
    {
        if (pageSequenceController != null)
        {
            pageSequenceController.CurrentIndex++;
        }
    }
}