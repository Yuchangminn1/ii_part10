using UnityEngine;

public class ReturnCheckScript : MonoBehaviour
{
    void OnEnable()
    {
        CustomSerialController.Instance.StartReturnChoice();
    }
}
