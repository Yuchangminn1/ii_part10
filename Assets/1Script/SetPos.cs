using UnityEngine;

public class SetPos : MonoBehaviour
{
    public RectTransform[] targetRect;
    RectTransform rectTransform;

    public int index = -1;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }



    void OnEnable()
    {
        if (ScoreManager.Instance == null) return;
        SetIndex(ScoreManager.Instance.GetLastStep());

        Debug.Log($"ScoreManager.Instance.GetLastStep() = {ScoreManager.Instance.GetLastStep()}");
        SetPosition();
    }

    public void SetIndex(int _index)
    {
        index = _index;
    }

    public void SetPosition()
    {
        if (rectTransform == null || index == -1)
        {
            Debug.Log("rectTransform == null || index == -1");
            return;
        }
        if (targetRect != null && targetRect.Length > 0)
        {
            Debug.Log("Index = {INdex}");
            rectTransform.localPosition = targetRect[index].localPosition;
        }
    }
}
