using UnityEngine;

/// <summary>
/// 도트 데미지 정보를 담는 Value Object
/// </summary>
public class BaseDotDamageVO
{
    public float damage;
    public int duration;  // 남은 프레임 수
    public int interval;  // 데미지 적용 간격 (프레임)
    public BaseHero owner;  // 데미지를 준 영웅 (통계 추적용)
    public int id;  // 도트 데미지 ID (같은 ID는 덮어쓰기)
    
    // Object Pool 패턴
    private static BaseDotDamageVO pooledInstance = null;
    
    public static BaseDotDamageVO GetVO()
    {
        if (pooledInstance == null)
        {
            pooledInstance = new BaseDotDamageVO();
        }
        
        // 재사용 전 초기화
        pooledInstance.damage = 0f;
        pooledInstance.duration = 0;
        pooledInstance.interval = 1;
        pooledInstance.owner = null;
        pooledInstance.id = -1;
        
        return pooledInstance;
    }
    
    public void Remove()
    {
        // 정리 작업
        damage = 0f;
        duration = 0;
        interval = 1;
        owner = null;
        id = -1;
    }
}