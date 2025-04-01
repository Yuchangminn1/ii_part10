using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetColorA : MonoBehaviour
{
    public bool[] flags;
    public List<Graphic> graphics;

    public bool[] raytargets;

    bool isInitialize = false;

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    Coroutine coroutine;

    void Awake()
    {
        isInitialize = false;

        if (graphics == null || graphics.Count < 1)
        {
            graphics.AddRange(GetComponentsInChildren<Graphic>());
        }
        flags = new bool[graphics.Count];
        raytargets = new bool[graphics.Count];

        for (int i = 0; i < graphics.Count; i++)
        {
            flags[i] = graphics[i].color.a > 0.5f;
        }
        for (int i = 0; i < graphics.Count; i++)
        {
            raytargets[i] = graphics[i].raycastTarget;
        }
        isInitialize = true;
    }
    void OnEnable()
    {
        if (FadeManager.Instance == null) return;
        if (coroutine == null) coroutine = StartCoroutine(WaitInitialize());
    }

    void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = null;
    }

    IEnumerator WaitInitialize()
    {
        yield return new WaitForSeconds(0.2f);

        while (!isInitialize) yield return waitForFixedUpdate;

        for (int i = 0; i < graphics.Count; i++)
        {
            if (flags[i])
            {
                FadeManager.Instance.SetAlphaOne(graphics[i]);
            }
            else
            {
                FadeManager.Instance.SetAlphaZero(graphics[i]);
                graphics[i].raycastTarget = false;

            }

        }
        yield return null;

    }
}
