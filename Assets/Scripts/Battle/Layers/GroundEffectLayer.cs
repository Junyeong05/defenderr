using UnityEngine;

// 바닥 효과 레이어 - 단순 컨테이너
public class GroundEffectLayer : MonoBehaviour
{
    private static GroundEffectLayer instance;
    
    public static GroundEffectLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GroundEffectLayer");
                instance = go.AddComponent<GroundEffectLayer>();
            }
            return instance;
        }
    }
}