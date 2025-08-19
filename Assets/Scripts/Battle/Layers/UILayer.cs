using UnityEngine;

// UI 레이어 - 단순 컨테이너
public class UILayer : MonoBehaviour
{
    private static UILayer instance;
    
    public static UILayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UILayer");
                instance = go.AddComponent<UILayer>();
            }
            return instance;
        }
    }
}