using UnityEngine;
using UnityEngine.Events;

public class NextTriigerButton : MonoBehaviour
{
    public int checkButtonIndex = -1;
    public UnityEvent triggerEvent;
    public void StartBuutonCheck()
    {
        if (triggerEvent == null || checkButtonIndex == -1)
        {
            Debug.Log("이벤트 없거나 버튼 인덱스 기입 안했음");
            return;
        }
        else
        {
            if (checkButtonIndex == 14)
            {
                CustomSerialController.Instance.BellTrigger();
                CustomSerialController.Instance.WaitForSerialAnswer(checkButtonIndex, SuccessTrigger2);

            }
            else CustomSerialController.Instance.WaitForSerialAnswer(checkButtonIndex, SuccessTrigger);
        }
    }

    void OnEnable()
    {
        if (PageController.Instance)
        {
            if (PageController.Instance.CurrentPage == 7)
            {
                StartBuutonCheck();

            }
        }
    }



    public void SuccessTrigger()
    {
        Debug.Log($"SuccessTrigger   {checkButtonIndex} ");
        triggerEvent?.Invoke();
    }
    public void SuccessTrigger2()
    {
        Debug.Log($"SuccessTrigger   {checkButtonIndex} ");
        CustomSerialController.Instance.SetAllwhite();
        PageController.Instance.NextButton();
    }
}
