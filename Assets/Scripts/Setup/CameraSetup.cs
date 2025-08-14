using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [Tooltip("원하는 가로 월드 단위")]
    public float targetWorldWidth = 640f;
    
    #if DEBUG || UNITY_EDITOR
    [Tooltip("디버그 로그 출력 여부")]
    public bool enableDebugLogs = false;
    #endif

    void Start()
    {
        SetupCamera();
    }

    void SetupCamera()
    {
        Camera cam = GetComponent<Camera>() ?? Camera.main;
        
        if (cam == null)
        {
            Debug.LogError("[CameraSetup] Camera not found!");
            return;
        }

        if (!IsValidScreenDimensions())
            return;

        cam.orthographic = true;
        
        float aspectRatio = GetValidAspectRatio();
        float orthographicSize = GetValidOrthographicSize(aspectRatio);
        
        cam.orthographicSize = orthographicSize;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        #if DEBUG || UNITY_EDITOR
        if (enableDebugLogs)
        {
            LogCameraInfo(cam, aspectRatio);
        }
        #endif
    }
    
    bool IsValidScreenDimensions()
    {
        if (Screen.width > 0 && Screen.height > 0)
            return true;
            
        Debug.LogWarning($"[CameraSetup] Invalid screen dimensions: {Screen.width}x{Screen.height}");
        return false;
    }
    
    float GetValidAspectRatio()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        
        if (!float.IsNaN(aspectRatio) && !float.IsInfinity(aspectRatio) && aspectRatio > 0)
            return aspectRatio;
            
        return 16f / 9f; // Default to 16:9
    }
    
    float GetValidOrthographicSize(float aspectRatio)
    {
        float size = targetWorldWidth / (2f * aspectRatio);
        
        if (!float.IsNaN(size) && !float.IsInfinity(size) && size > 0)
            return size;
            
        return 180f; // Default size
    }
    
    #if DEBUG || UNITY_EDITOR
    void LogCameraInfo(Camera cam, float aspectRatio)
    {
        float worldWidth = cam.orthographicSize * 2f * aspectRatio;
        float worldHeight = cam.orthographicSize * 2f;
        
        Debug.Log($"[CameraSetup] Camera configured - " +
                  $"Screen: {Screen.width}x{Screen.height}, " +
                  $"World: {worldWidth:F0}x{worldHeight:F0} units");
    }
    #endif

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetupCamera();
        }
    }
    #endif
}
