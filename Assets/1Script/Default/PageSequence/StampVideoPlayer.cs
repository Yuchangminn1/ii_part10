using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StampVideoPlayer : MonoBehaviour
{
    public Graphic graphic;
    public VideoPlayer[] stampVideos;

    public WaitForSeconds wiatforsecond = new WaitForSeconds(0.1f);
    void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    void OnEnable()
    {
        if (graphic != null && FadeManager.Instance)
        {
            FadeManager.Instance.SetAlphaZero(graphic);
        }
        for (int i = 0; i < 3; i++)
        {

            if (IsVideoPlayerActive(stampVideos[i]) == false)
            {
                Debug.Log("비디오 플레이어 꺼져있음 ");
                return;

            }
        }

        StartCoroutine(QQQ());

    }

    /// <summary>
    /// 지정된 VideoPlayer 오브젝트의 활성 상태(SetActive)를 반환합니다.
    /// </summary>
    public bool IsVideoPlayerActive(VideoPlayer vp)
    {
        return vp.gameObject.activeSelf;
    }

    IEnumerator QQQ()
    {
        yield return new WaitForSeconds(1f);
        if (CustomSerialController.Instance)
        {
            if (CustomSerialController.Instance.checkNum > 8)
            {

                stampVideos[2].Prepare();

                if (stampVideos[2].isPrepared == false)
                {
                    yield return wiatforsecond;
                }
                stampVideos[2].Play();
                yield return wiatforsecond;
                FadeManager.Instance.SetAlphaOne(graphic);


                UserDataManager.Instance.RequestUserDataUpdate(1, $"{3}");
            }
            else if (CustomSerialController.Instance.checkNum > 4)
            {
                stampVideos[1].Prepare();

                if (stampVideos[1].isPrepared == false)
                {
                    yield return wiatforsecond;
                }
                stampVideos[1].Play();
                yield return wiatforsecond;

                FadeManager.Instance.SetAlphaOne(graphic);


                UserDataManager.Instance.RequestUserDataUpdate(1, $"{2}");
            }
            else if (CustomSerialController.Instance.checkNum > -1)
            {
                stampVideos[0].Prepare();

                if (stampVideos[0].isPrepared == false)
                {
                    yield return wiatforsecond;
                }
                stampVideos[0].Play();
                yield return wiatforsecond;

                FadeManager.Instance.SetAlphaOne(graphic);


                UserDataManager.Instance.RequestUserDataUpdate(1, $"{1}");
            }
            CustomSerialController.Instance.checkNum = 0;

        }

    }
}
