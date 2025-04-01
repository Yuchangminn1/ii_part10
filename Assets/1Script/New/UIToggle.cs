using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour
{
    [SerializeField] List<Graphic> graphics = new List<Graphic>();

    Color colorWhite = Color.white;
    Color alphaZero = new Color(1, 1, 1, 0);

    public bool[] flag;

    public GameObject[] target;

    public bool skipOnEnable;

    void Awake()
    {
        if (graphics.Count == 0)
        {
            Graphic _graphic = GetComponent<Graphic>();
            if (_graphic != null)
                graphics.Add(_graphic);
        }


        TargetGraphicsSet();


        FlagInitialize();
    }

    private void FlagInitialize()
    {
        if (flag == null || flag.Length < 1)
        {
            flag = new bool[graphics.Count];  // flag false / color.a = 1 ->0

            for (int i = 0; i < graphics.Count; i++)
            {
                if (colorWhite == graphics[i].color)
                {
                    flag[i] = false;
                }
                else
                {
                    flag[i] = true;
                }

            }
        }

    }

    private void TargetGraphicsSet()
    {
        if (target != null)
        {
            for (int i = 0; i < target.Length; i++)
            {
                graphics.AddRange(target[i].GetComponentsInChildren<Graphic>());
            }
        }
    }

    void OnEnable()
    {
        if (skipOnEnable) return;
        if (flag != null && flag.Length > 0)
        {
            for (int i = 0; i < graphics.Count; i++)
            {
                if (flag[i]) graphics[i].color = alphaZero;

                else graphics[i].color = colorWhite;
            }
        }
    }


    public void Change()
    {
        foreach (var graphic in graphics)
        {
            if (graphic.color.a > 0.1)
            {
                graphic.color = alphaZero;
                graphic.raycastTarget = false;
                continue;
            }

            if (graphic.color.a < 0.1f)
            {
                graphic.color = colorWhite;
                graphic.raycastTarget = true;
            }
        }
    }

}
