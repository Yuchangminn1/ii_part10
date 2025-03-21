using System.Collections;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    public int count = 0;

    Coroutine coroutine;

    WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

    public void ExitBuuton()
    {
        if (count == 0)
        {
            if (coroutine == null) coroutine = StartCoroutine(ExitReset());
        }
        count++;
        if (count > 4)
            Application.Quit();
    }

    IEnumerator ExitReset()
    {
        yield return waitForSeconds;
        count = 0;

        coroutine = null;
    }


}
