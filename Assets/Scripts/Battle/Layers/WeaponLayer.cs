using UnityEngine;

// 무기(투사체) 레이어 - 화살, 마법 등을 관리하는 컨테이너
public class WeaponLayer : MonoBehaviour
{
    private static WeaponLayer instance;
    
    public static WeaponLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("WeaponLayer");
                instance = go.AddComponent<WeaponLayer>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}