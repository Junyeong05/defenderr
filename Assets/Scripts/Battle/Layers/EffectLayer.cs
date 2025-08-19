using UnityEngine;

// 효과 레이어 - 단순 컨테이너
public class EffectLayer : MonoBehaviour
{
    private static EffectLayer instance;
    
    public static EffectLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EffectLayer");
                instance = go.AddComponent<EffectLayer>();
            }
            return instance;
        }
    }
}