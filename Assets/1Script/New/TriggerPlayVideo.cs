using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class TriggerPlayVideo : MonoBehaviour
{
    [SerializeField] UnityEvent onUpArrow;
    [SerializeField] UnityEvent onDownArrow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            onUpArrow?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            onDownArrow?.Invoke();
        }
    }
}
