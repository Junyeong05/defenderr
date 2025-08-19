using UnityEngine;

// 배경 레이어 - 단순 컨테이너
public class BackgroundLayer : MonoBehaviour
{
    private static BackgroundLayer instance;
    
    public static BackgroundLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("BackgroundLayer");
                instance = go.AddComponent<BackgroundLayer>();
            }
            return instance;
        }
    }
}