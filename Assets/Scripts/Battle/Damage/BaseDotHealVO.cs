using UnityEngine;

/// <summary>
/// 도트 힐 정보를 담는 Value Object
/// </summary>
public class BaseDotHealVO
{
    public float heal;
    public int duration;  // 남은 프레임 수
    public int interval;  // 힐 적용 간격 (프레임)
    public BaseHero owner;  // 힐을 준 영웅 (통계 추적용)
    
    // Object Pool 패턴
    private static BaseDotHealVO pooledInstance = null;
    
    public static BaseDotHealVO GetVO()
    {
        if (pooledInstance == null)
        {
            pooledInstance = new BaseDotHealVO();
        }
        
        // 재사용 전 초기화
        pooledInstance.heal = 0f;
        pooledInstance.duration = 0;
        pooledInstance.interval = 1;
        pooledInstance.owner = null;
        
        return pooledInstance;
    }
    
    public void Remove()
    {
        // 정리 작업
        heal = 0f;
        duration = 0;
        interval = 1;
        owner = null;
    }
}