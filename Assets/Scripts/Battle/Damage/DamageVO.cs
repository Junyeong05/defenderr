using UnityEngine;

/// <summary>
/// 데미지 계산 결과를 담는 Value Object
/// </summary>
public class DamageVO
{
        public float damage = 0f;
        public bool isCritical = false;
        public float shield = 0f;
        public float shieldWithDuration = 0f;

        // Object Pool 패턴을 위한 static 메서드
        private static DamageVO pooledInstance = null;
        
        public static DamageVO GetVO()
        {
            if (pooledInstance == null)
            {
                pooledInstance = new DamageVO();
            }
            
            // 재사용 전 초기화
            pooledInstance.damage = 0f;
            pooledInstance.isCritical = false;
            pooledInstance.shield = 0f;
            pooledInstance.shieldWithDuration = 0f;
            
            return pooledInstance;
        }
        
        public void Reset()
        {
            damage = 0f;
            isCritical = false;
            shield = 0f;
            shieldWithDuration = 0f;
        }
    }