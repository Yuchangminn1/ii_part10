using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class ActivateAllDisplays : MonoBehaviour
{

    void Start()
    {
        Debug.Log("연결된 디스플레이 수: " + Display.displays.Length);

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log($"Display {i} 활성화됨");
        }

        // 기본 디스플레이: 해상도에 따라 전체화면 또는 창모드 설정
        if (Display.displays[0].systemWidth == 1080 && Display.displays[0].systemHeight == 1920)
        {
            Screen.SetResolution(1080, 1920, true); // 전체화면
            Debug.Log("Display 0: 전체화면 (1080x1920)");
        }
        else
        {
            Screen.SetResolution(1080 / 2, 1920 / 2, false); // 창모드
            Debug.Log("Display 0: 창모드 (1080x1920)");
        }

        // 추가 디스플레이가 있으면 처리
        if (Display.displays.Length > 2)
        {
            var disp2 = Display.displays[2];
            if (disp2.systemWidth == 1920 && disp2.systemHeight == 1080)
            {
                disp2.Activate(1920, 1080, 60); // 전체화면 (보조는 무조건 전체화면)
                Debug.Log("Display 2: 전체화면 (1920x1080)");
            }
            else
            {
                // 창모드처럼 보이게 해상도 줄여서 활성화 (제한적)
                disp2.Activate(1280, 720, 60); // 창모드처럼 보이게 설정
                Debug.Log("Display 2: 창모드처럼 Activate (1280x720)");
            }
        }
    }
}
