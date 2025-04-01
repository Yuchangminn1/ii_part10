using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TestVideoSpeed : MonoBehaviour
{
    [SerializeField] List<VideoPlayer> videoPlayer = new List<VideoPlayer>();

    public float speed = 1;

    void Awake()
    {
        videoPlayer.AddRange(GetComponentsInChildren<VideoPlayer>());
    }

    void Start()
    {
        for (int i = 0; i < videoPlayer.Count; i++)
        {
            videoPlayer[i].playbackSpeed *= speed;

        }
    }
}
