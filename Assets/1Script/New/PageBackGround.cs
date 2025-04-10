using UnityEngine;
using UnityEngine.UI;

public class PageBackGround : MonoBehaviour
{
    [SerializeField] Graphic[] graphics;

    [SerializeField] Graphic currentBackGround;


    public void ChangeBG(int _pageNum)
    {
        if (graphics.Length < _pageNum)
        {
            Debug.Log("ChangeBG Error");
            return;
        }

        if (currentBackGround != null && currentBackGround.color.a > 0.5f)
        {
            FadeManager.Instance.TargetFade(currentBackGround, 0);
        }
        currentBackGround = graphics[_pageNum];
        FadeManager.Instance.TargetFade(currentBackGround, 1);

    }



}
