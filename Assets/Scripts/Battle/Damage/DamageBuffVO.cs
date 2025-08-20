/// <summary>
/// 데미지 계산에 사용되는 버프 수치들
/// </summary>
public class DamageBuffVO
{
        public float critChanceUp = 0f;        // 크리티컬 확률 증가
        public float critMultiplierUp = 0f;    // 크리티컬 배수 증가
        public float penetrateUp = 0f;          // 방어 관통 증가
        public float damageUpForShield = 0f;   // 보호막에 대한 추가 데미지
        
        // Object Pool 패턴
        private static DamageBuffVO pooledInstance = null;
        
        public static DamageBuffVO GetVO()
        {
            if (pooledInstance == null)
            {
                pooledInstance = new DamageBuffVO();
            }
            
            // 재사용 전 초기화
            pooledInstance.critChanceUp = 0f;
            pooledInstance.critMultiplierUp = 0f;
            pooledInstance.penetrateUp = 0f;
            pooledInstance.damageUpForShield = 0f;
            
            return pooledInstance;
        }
        
        public void Reset()
        {
            critChanceUp = 0f;
            critMultiplierUp = 0f;
            penetrateUp = 0f;
            damageUpForShield = 0f;
        }
    }