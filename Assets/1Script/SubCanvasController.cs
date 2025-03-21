using UnityEngine;

public class SubCanvasController : MonoBehaviour
{
    [SerializeField] GameObject subCanvas;
    void OnEnable()
    {
        if (subCanvas != null) subCanvas.SetActive(true);
    }

    void OnDisable()
    {
        if (subCanvas != null) subCanvas.SetActive(false);
    }
}
