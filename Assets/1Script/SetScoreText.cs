using UnityEngine;
using UnityEngine.UI;

public class SetScoreText : MonoBehaviour
{
    Text text;
    void Awake()
    {
        text = GetComponent<Text>();
    }
    void OnEnable()
    {
        if (text != null)
            text.text = ScoreManager.Instance.GetNumberOfCorrects().ToString();
    }
}
