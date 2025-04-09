using System;
using UnityEngine;
using UnityEngine.UI;

public class CopyImage : MonoBehaviour
{
    public Image[] targetImages;
    public Image currentImage;

    void OnEnable()
    {
        if (FadeManager.Instance == null) return;
        foreach (Image target in targetImages)
        {
            target.sprite = currentImage.sprite;
            //FadeManager.Instance.SetAlphaOne(target);
        }
    }

    private void Awake()
    {
        currentImage = GetComponent<Image>();
    }
}