using System.Collections;
using UnityEngine;

public class UserReset : MonoBehaviour
{
    public int count = 0;

    public int resetNum = 5;

    Coroutine coroutine;

    WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

    public void ResetBuuton()
    {
        if (count == 0)
        {
            if (coroutine == null) coroutine = StartCoroutine(ExitReset());
        }
        count++;
        if (count >= resetNum)
            UserDataManager.Instance.ResetUser();
    }

    IEnumerator ExitReset()
    {
        yield return waitForSeconds;
        count = 0;

        coroutine = null;
    }
}
