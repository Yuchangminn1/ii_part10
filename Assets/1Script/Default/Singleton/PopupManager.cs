using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{

    public static PopupManager Instance { get; private set; }


    [SerializeField] Text[] popupTextS;

    [Header("팝업 텍스트 할당필요")]
    [SerializeField] Text text;

    Image imageBG;
    [Header("팝업 이미지 할당필요")]
    [SerializeField] Image popupImage;

    Color32 imagecolor32 = new Color32(0, 0, 0, 76);

    Coroutine coroutine;

    int index = -1;

    private void Awake()
    {
        Instance = this;
        imageBG = GetComponent<Image>();

        StartCoroutine(delayAwake());


    }
    public void SetText(int _textIndex)
    {
        if (text == null) return;
        if (_textIndex < popupTextS.Length)
            text.text = popupTextS[_textIndex].text;
    }

    public int GetIndex()
    {
        return index;
    }

    public void ResetIndex()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        index = -1;
        FadeManager.Instance.SetAlphaZero(imageBG);
        FadeManager.Instance.SetAlphaZero(popupImage);
        FadeManager.Instance.SetAlphaZero(text);
    }

    public void Popup(int _textIndex = 0)
    {
        if (index == 0 && _textIndex == 0)
            index = 1;
        else
            index = _textIndex;
        SetText(index);
        if (coroutine == null) coroutine = StartCoroutine(PopupCor());
    }
    public int RPopup(int _textIndex = 0)
    {
        if (index == 0 && _textIndex == 0)
            index = 1;
        else
            index = _textIndex;
        SetText(index);
        if (coroutine == null) coroutine = StartCoroutine(PopupCor());
        return index;
    }

    IEnumerator PopupCor()
    {
        imageBG.color = imagecolor32;
        FadeManager.Instance.SetAlphaOne(popupImage);
        FadeManager.Instance.SetAlphaOne(text);
        yield return null;
        coroutine = null;

    }
    void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator delayAwake()
    {
        yield return new WaitForSeconds(1f);
        FadeManager.Instance.SetAlphaZero(imageBG);
        FadeManager.Instance.SetAlphaZero(popupImage);
        FadeManager.Instance.SetAlphaZero(text);
        FadeManager.Instance.SetAlphaZero(popupTextS);

    }

}
