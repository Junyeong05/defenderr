using UnityEngine;

/// <summary>
/// 마우스 입력 안전 처리 유틸리티
/// Screen position out of view frustum 오류 방지
/// </summary>
public static class SafeMouseInput
{
    /// <summary>
    /// 안전한 ScreenPointToRay 호출
    /// </summary>
    public static bool TryScreenPointToRay(Camera camera, Vector3 mousePosition, out Ray ray)
    {
        ray = default;
        
        // 카메라 유효성 체크
        if (camera == null)
            return false;
        
        // 마우스 위치가 화면 영역 내에 있는지 체크
        if (mousePosition.x < 0 || mousePosition.x > Screen.width ||
            mousePosition.y < 0 || mousePosition.y > Screen.height)
        {
            return false;
        }
        
        // (0,0) 위치 필터링 (Unity 에디터 버그)
        if (mousePosition.x == 0 && mousePosition.y == 0)
        {
            return false;
        }
        
        try
        {
            ray = camera.ScreenPointToRay(mousePosition);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ScreenPointToRay failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 마우스 위치가 유효한지 체크
    /// </summary>
    public static bool IsMousePositionValid()
    {
        Vector3 mousePos = Input.mousePosition;
        
        // 화면 영역 체크
        if (mousePos.x < 0 || mousePos.x > Screen.width ||
            mousePos.y < 0 || mousePos.y > Screen.height)
        {
            return false;
        }
        
        // (0,0) 버그 체크
        if (mousePos.x == 0 && mousePos.y == 0)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 안전한 마우스 위치 가져오기
    /// </summary>
    public static Vector3 GetSafeMousePosition()
    {
        if (IsMousePositionValid())
        {
            return Input.mousePosition;
        }
        
        // 기본값으로 화면 중앙 반환
        return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    }
}