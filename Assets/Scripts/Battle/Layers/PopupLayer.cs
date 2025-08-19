using UnityEngine;

// 팝업 레이어 - 단순 컨테이너
public class PopupLayer : MonoBehaviour
{
    private static PopupLayer instance;
    
    public static PopupLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PopupLayer");
                instance = go.AddComponent<PopupLayer>();
            }
            return instance;
        }
    }
}