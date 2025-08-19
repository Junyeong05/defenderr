using UnityEngine;

// 유닛(영웅) 레이어 - 단순 컨테이너
public class UnitLayer : MonoBehaviour
{
    private static UnitLayer instance;
    
    public static UnitLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UnitLayer");
                instance = go.AddComponent<UnitLayer>();
            }
            return instance;
        }
    }
}