using UnityEngine;

[ExecuteAlways]
public class CameraSetup : MonoBehaviour
{
    [Tooltip("원하는 가로 월드 단위")]
    public float targetWorldWidth = 640f;

    void Awake()
    {
        // 1) 메인 카메라 참조
        Camera cam = Camera.main;
        cam.orthographic = true;

        // 2) 화면 비율 계산
        float aspectRatio = (float)Screen.width / Screen.height;
        
        // 3) 가로 640에 맞는 OrthographicSize 계산
        // 가로 = OrthographicSize * 2 * aspectRatio = 640
        // OrthographicSize = 640 / (2 * aspectRatio)
        cam.orthographicSize = targetWorldWidth / (2f * aspectRatio);

        // 4) 카메라 위치 조정 - (0,0)를 화면 중앙에 두기
        cam.transform.position = new Vector3(0f, 0f, -10f);

        // 5) 실제 가시 범위 계산
        float actualWorldWidth = cam.orthographicSize * 2f * aspectRatio;
        float actualWorldHeight = cam.orthographicSize * 2f;
    }
}
