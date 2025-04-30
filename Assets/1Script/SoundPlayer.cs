using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public bool isBGM = false;
    public AudioSource suondsource;

    void OnEnable()
    {
        if (isBGM)
            suondsource?.Play();
    }
    void OnDisable()
    {
        if (isBGM)
            suondsource?.Stop();

    }
}
