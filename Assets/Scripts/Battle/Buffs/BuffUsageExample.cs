using UnityEngine;

// 버프 시스템 사용 예시
public class BuffUsageExample : MonoBehaviour
{
    // 예시 1: 아군이 공격력 버프를 주는 경우
    void ApplyAllyDamageBuff(BaseHero targetHero)
    {
        // A타입 5레벨 아군이 10% 공격 버프 (300프레임 = 5초)
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.1f, 300);
        
        // A타입 10레벨 아군이 15% 공격 버프 (300프레임)
        // 같은 타입이므로 더 큰 값인 15%만 적용됨
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.15f, 300);
        
        // B타입 아군이 20% 공격 버프 (180프레임 = 3초)
        // 다른 타입이므로 A타입 15% + B타입 20% = 총 35% 증가
        targetHero.AddDamageBuff(BuffType.DAMAGE_B, 0.2f, 180);
    }
    
    // 예시 2: 적이 디버프를 거는 경우
    void ApplyEnemyDebuffs(BaseHero targetHero)
    {
        // 동상으로 이동속도 30% 감소 (120프레임 = 2초)
        targetHero.AddMoveSpeedBuff(BuffType.SLOW_FROST, -0.3f, 120);
        
        // 화상으로 이동속도 20% 감소 (60프레임 = 1초)
        // 다른 타입이므로 동상 30% + 화상 20% = 총 50% 감소
        targetHero.AddMoveSpeedBuff(BuffType.SLOW_FIRE, -0.2f, 60);
        
        // 공격력 25% 감소 (180프레임 = 3초)
        targetHero.AddDamageBuff(BuffType.WEAKEN_A, -0.25f, 180);
    }
    
    // 예시 3: 같은 타입 버프 중복 시 최대값 적용
    void DemonstrateBuffStacking(BaseHero targetHero)
    {
        // 3명의 A타입 아군이 각각 다른 공격 버프를 줌
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.05f, 300); // 5%
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.10f, 300); // 10%
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.07f, 300); // 7%
        // 결과: A타입은 최대값인 10%만 적용
        
        // 2명의 B타입 아군이 버프를 줌
        targetHero.AddDamageBuff(BuffType.DAMAGE_B, 0.15f, 200); // 15%
        targetHero.AddDamageBuff(BuffType.DAMAGE_B, 0.12f, 250); // 12%
        // 결과: B타입은 최대값인 15%만 적용
        
        // 최종 공격력 증가: A타입 10% + B타입 15% = 25%
    }
    
    // 예시 4: 버프 지속시간 관리
    void DemonstrateBuffDuration(BaseHero targetHero)
    {
        // 짧은 강력한 버프 vs 긴 약한 버프
        
        // 강하지만 짧은 버프 (50% 증가, 1초)
        targetHero.AddAttackSpeedBuff(BuffType.ATTACK_SPEED_A, 0.5f, 60);
        
        // 약하지만 긴 버프 (20% 증가, 5초)
        // 처음 1초간은 50%가 적용되고, 이후 4초간은 20%가 적용됨
        targetHero.AddAttackSpeedBuff(BuffType.ATTACK_SPEED_A, 0.2f, 300);
    }
    
    // 예시 5: 버프 제거
    void RemoveBuffsExample(BaseHero targetHero)
    {
        // 모든 버프/디버프 제거
        targetHero.RemoveAllBuffs();
        
        // 디버프만 제거 (정화 스킬 등)
        targetHero.RemoveAllDebuffs();
    }
    
    // 예시 6: 특정 버프 강제 덮어쓰기
    void OverrideBuffExample(BaseHero targetHero)
    {
        // 일반 버프 적용
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.1f, 300);
        
        // 특수 스킬로 강제 덮어쓰기 (override = true)
        // 기존 값과 지속시간 무관하게 새 값으로 교체
        targetHero.AddDamageBuff(BuffType.DAMAGE_A, 0.5f, 60, true);
    }
}