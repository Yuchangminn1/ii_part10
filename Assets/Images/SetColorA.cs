using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetColorA : MonoBehaviour
{
    public bool[] flags;
    public List<Graphic> graphics;

    void Awake()
    {


        if (graphics == null || graphics.Count < 1)
        {
            graphics.AddRange(GetComponentsInChildren<Graphic>());
        }
        flags = new bool[graphics.Count];

        for (int i = 0; i < graphics.Count; i++)
        {
            flags[i] = graphics[i].color.a > 0.5f;
        }
    }
    void OnEnable()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            if (flags[i])
            {
                FadeManager.Instance.SetAlphaOne(graphics[i]);
            }
            else
            {
                FadeManager.Instance.SetAlphaZero(graphics[i]);
            }
        }
    }
}
